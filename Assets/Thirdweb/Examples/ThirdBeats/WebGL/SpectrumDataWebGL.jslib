var SpectrumDataWebGL = {
    $analyzers: Object.create(null),
    $audioContext: null,
    $globalAudioData: Object.create(null),
    $audioDataCache: Object.create(null),
    $lastUpdateTime: Object.create(null),

    StartSampling: function(namePtr, bufferSize, sampleRate) {
        var name = UTF8ToString(namePtr);
        if (analyzers[name]) {
            return true;
        }

        try {
            if (!audioContext) {
                audioContext = new (window.AudioContext || window.webkitAudioContext)();
            }
            var analyzer = audioContext.createAnalyser();
            analyzer.fftSize = bufferSize * 2;

            analyzers[name] = {
                analyzer: analyzer,
                buffer: new Float32Array(bufferSize),
                audioBuffer: audioContext.createBuffer(1, bufferSize, sampleRate),
                source: null,
                sampleRate: sampleRate,
                bufferSize: bufferSize
            };

            globalAudioData[name] = new Float32Array(bufferSize); // Initialize global audio data

            return true;
        } catch (e) {
            console.error("Failed to start sampling:", e);
            return false;
        }
    },

    ProvideAudioData: function(namePtr, bufferPtr, bufferSize, sampleRate) {
        var name = UTF8ToString(namePtr);
        var audioData = new Float32Array(Module.HEAPF32.buffer, bufferPtr, bufferSize);

        if (!analyzers[name]) {
            console.error("No analyzer found for", name);
            return false;
        }

        try {
            var analyzerObj = analyzers[name];
            var currentTime = Date.now();

            // Throttle updates to every 500ms to reduce load
            if (lastUpdateTime[name] && currentTime - lastUpdateTime[name] < 500) {
                return true;
            }
            lastUpdateTime[name] = currentTime;

            // Cache the new audio data if significantly different
            var cachedData = audioDataCache[name];
            if (!cachedData || cachedData.length !== audioData.length || !cachedData.every((v, i) => v === audioData[i])) {
                audioDataCache[name] = new Float32Array(audioData);

                if (!analyzerObj.audioBuffer || analyzerObj.audioBuffer.length !== bufferSize) {
                    analyzerObj.audioBuffer = audioContext.createBuffer(1, bufferSize, sampleRate);
                }

                // Process audio data in a single step
                analyzerObj.audioBuffer.copyToChannel(audioData, 0);

                if (analyzerObj.source) {
                    analyzerObj.source.stop();
                    analyzerObj.source.disconnect();
                    analyzerObj.source = null;
                }

                analyzerObj.source = audioContext.createBufferSource();
                analyzerObj.source.buffer = analyzerObj.audioBuffer;
                analyzerObj.source.connect(analyzerObj.analyzer);
                analyzerObj.source.start();
                analyzerObj.source.onended = function() {
                    if (analyzerObj.source) {
                        analyzerObj.source.disconnect();
                        analyzerObj.source = null;
                    }
                };
            }

            return true;
        } catch (e) {
            console.error("Failed to provide audio data:", e);
            return false;
        }
    },

    GetSamples: function(namePtr, bufferPtr, bufferSize) {
        var name = UTF8ToString(namePtr);
        if (!analyzers[name]) {
            console.error("No analyzer found for", name);
            return false;
        }

        try {
            var buffer = new Float32Array(Module.HEAPF32.buffer, bufferPtr, bufferSize);
            var analyzerObj = analyzers[name];
            analyzerObj.analyzer.getFloatTimeDomainData(buffer);
            return true;
        } catch (e) {
            console.error("Failed to get spectrum sample data:", e);
            return false;
        }
    }
};

autoAddDeps(SpectrumDataWebGL, '$analyzers');
autoAddDeps(SpectrumDataWebGL, '$audioContext');
autoAddDeps(SpectrumDataWebGL, '$globalAudioData');
autoAddDeps(SpectrumDataWebGL, '$audioDataCache');
autoAddDeps(SpectrumDataWebGL, '$lastUpdateTime');
mergeInto(LibraryManager.library, SpectrumDataWebGL);
