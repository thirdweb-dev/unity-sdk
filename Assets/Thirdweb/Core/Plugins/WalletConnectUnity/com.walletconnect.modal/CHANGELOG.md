# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.6] - 2024-05-09

### Fixed

- `WalletConnectModal` doesn't respect `ResumeSessionOnInit` setting; always tries to resume session on init

## [1.1.3] - 2024-04-16

### Added

- Trim spaces wallet search query

### Fixed

- SignClient disposing when duplicate WalletConnectModal deletion occurs
- WalletSearchView resetting to default null WCListSelect cards

## [1.1.0] - 2024-03-21

### Changed

- Update sample
- Upgrade `com.walletconnect.core` to v3.1.0
- Upgrade `com.walletconnect.ui` to v1.1.0