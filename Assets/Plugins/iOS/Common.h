#ifndef Common_h
#define Common_h

typedef int bool_t;

inline bool_t toBool(bool v)
{
    return v ? 1 : 0;
}

inline bool toBool(bool_t v)
{
    return v != 0;
}

inline NSString* toString(const char* string)
{
    if (string != NULL)
    {
        return [NSString stringWithUTF8String:string];
    }
    else
    {
        return [NSString stringWithUTF8String:""];
    }
}

inline char* toString(NSString* string)
{
    const char* cstr = [string UTF8String];
    
    if (cstr == NULL)
        return NULL;
    
    char* copy = (char*)malloc(strlen(cstr) + 1);
    strcpy(copy, cstr);
    return copy;
}

#endif /* Common_h */
