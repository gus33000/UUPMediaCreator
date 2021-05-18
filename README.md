<p align="center">
  <img src="Assets/logo.png" width="400"><br>
  <b>An utility to convert Unified Update Platform file sets into Windows Media files</b><br>
  <a href="./src">Source Code</a> |
  <a href="./docs">Documentation</a> |
  <a href="./thirdparty">Third party libraries</a>
  <br><br>
  <img src="https://github.com/gus33000/UUPMediaCreator/actions/workflows/ci.yml/badge.svg"><img src="https://github.com/gus33000/UUPMediaCreator/actions/workflows/uwp.yml/badge.svg"><br><br>
  <img src="Assets/screenshot.png"><br><br>
</p>

## Readme

UUP Media Creator is a set of tools designed to help you scan, fetch updates from Microsoft own Unified Update Platform (UUP), and allow you to create medium out of it when an appropriate tool does not exist in the wild.

Right now the tooling includes:

- UUPDownload: Allows you to scan, fetch and download updates from UUP (Windows Desktop, Windows 10X, Windows Holographic, Windows Server, Windows Mobile, Windows IoT etc...)
- UUPMediaConverterCli: Allows you to convert a downloaded UUP update for Windows Desktop into an usable ISO image to use in last decade DVD reader or simply mounted.
- An experimental UWP front end is also provided, but is not the main focus for now.

## Supported features by OS

| Feature                                                                                  | Windows | Linux  | macOS  |
|------------------------------------------------------------------------------------------|---------|--------|--------|
| Downloading files from UUP (UUPDownload)                                                 | ✅       | ✅       | ✅       |
| Replaying past downloads from UUP (UUPDownload)                                          | ✅       | ✅       | ✅       |
| Decrypting ESRP payloads from UUP (UUPDownload)                                          | ✅       | ✅       | ✅       |
| Verifying downloaded payloads from UUP (UUPDownload)                                     | ✅       | ✅       | ✅       |
| Sample available builds from UUP (get-builds argument) (UUPDownload)                     | ✅       | ✅       | ✅       |
| Converting Desktop UUP files to an ISO for one base edition (UUPMediaConverterCli)       | ✅       | ✅       | ✅       |
| Does not require administrative privileges for base edition ISO images                   | ✅       | ✅       | ✅       |
| CLI available                                                                            | ✅       | ✅       | ✅       |
| GUI available                                                                            | ✅       | ❌       | ❌       |
| Converting Desktop UUP files to an ISO for all possible editions (UUPMediaConverterCli)  | ✅       | ❌       | ❌       |
| ISO Preinstallation Environment close to original (UUPMediaConverterCli)                 | ✅       | ❌       | ❌       |
| ISO built by CDImage Mastering Utility with UDF and boot sector (UUPMediaConverterCli)   | ✅       | ❌       | ❌       |
| ISO built by mkisofs with UDF and boot sector (UUPMediaConverterCli)                     | ❌       | ✅   (2) | ✅   (3) |
| Update file integration (UUPMediaConverterCli)                                           | ❌       | ❌       | ❌       |
| Building Hololens, Mobile, 10X, IoT images                                               | ❌       | ❌       | ❌       |
| UUPDownload ease of use (will come soon, we promise)                                     | ❌       | ❌       | ❌       |
| ARM64 Support (aka works on @sinclairinat0r device (TM))                                 | ✅       | ✅       | ❌  (1)  |

(1): Can be ran through Rosetta

(2): requires ```apt-get install genisoimage```

(3): requires ```brew install cdrtools```

## Usage

UUPDownload might be a little too complicated to use for some people. For reference purposes, here's the current set of parameters you can use as of ```2021-05-18```:

| Channel                   | Command |
|---------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Retail (Desktop)          | -s Professional -v 10.0.19043.1 -r Retail -b Retail -c vb_release -t [your architecture] |
| Retail (IoT)              | -s IoTUAP -v 10.0.17763.1 -r Retail -b Retail -c rs5_release -t [your architecture] |
| Retail (Holographic)      | -s Holographic -v 10.0.20346.1 -r Retail -b Retail -c fe_release -t [your architecture] |
| Retail (Mobile)           | -s MobileCore -v 10.0.15254.1 -r Retail -b Retail -c feature2 -t [your architecture] |
| Retail (Team)             | -s PPIPro -v 10.0.19043.1 -r Retail -b Retail -c vb_release -t [your architecture] |
| Release Preview (Desktop) | -s Professional -v 10.0.19043.1 -r External -b ReleasePreview -c vb_release -t [your architecture] |
| Beta (Desktop)            | -s Professional -v 10.0.19043.1 -r External -b Beta -c vb_release -t [your architecture] |
| Beta (Holographic)        | -s Holographic -v 10.0.19043.1 -r External -b Beta -c vb_release -t [your architecture] |
| Beta (Team)               | -s PPIPro -v 10.0.19043.1 -r External -b Beta -c vb_release -t [your architecture] |
| Dev (Desktop)             | -s Professional -v 10.0.19043.1 -r External -b Dev -c vb_release -t [your architecture] |
| Dev (Holographic)         | -s Holographic -v 10.0.19043.1 -r External -b Dev -c vb_release -t [your architecture] |
| Dev (Team)                | -s PPIPro -v 10.0.19043.1 -r External -b Dev -c vb_release -t [your architecture] |

**TIP 1:** You can append ```-e [Edition to download]``` to get the files needed only for a specific edition

**TIP 2:** You can append ```-l [Language to download]``` to get the files needed only for a specific language

## Contributing

You are free to contribute to this project, in fact we would welcome any form of help. But please keep in mind the following points if you do:

- Please try to maintain your PR with a reasonable set of commits
- Please try to not change functionality not concerned by your PR
- Avoid changing the entire app, if you feel like this is needed, please file an issue so we can discuss about it between contributors.

## Bugs

This project, as with anything in Computer Science, is not bug free. If you find anything wrong, please file an issue in the issue tracker. Please include the tool version, as well as the Operating System you are running on your computer, and any other applicable details helpful to reproduce the issue.
