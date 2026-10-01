<div align="center">

# Solar UI » Utilities & Integrations

</div>

<div align="left">

[![Release](https://img.shields.io/github/v/release/SolarDyn/Solar-UI?include_prereleases&style=flat&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxNiAxNiIgd2lkdGg9IjE2IiBoZWlnaHQ9IjE2IiBmaWxsPSIjZmZmZmZmIj48cGF0aCBkPSJNMSA3Ljc3NVYyLjc1QzEgMS43ODQgMS43ODQgMSAyLjc1IDFoNS4wMjVjLjQ2NCAwIC45MS4xODQgMS4yMzguNTEzbDYuMjUgNi4yNWExLjc1IDEuNzUgMCAwIDEgMCAyLjQ3NGwtNS4wMjYgNS4wMjZhMS43NSAxLjc1IDAgMCAxLTIuNDc0IDBsLTYuMjUtNi4yNUExLjc1MiAxLjc1MiAwIDAgMSAxIDcuNzc1Wm0xLjUgMGMwIC4wNjYuMDI2LjEzLjA3My4xNzdsNi4yNSA2LjI1YS4yNS4yNSAwIDAgMCAuMzU0IDBsNS4wMjUtNS4wMjVhLjI1LjI1IDAgMCAwIDAtLjM1NGwtNi4yNS02LjI1YS4yNS4yNSAwIDAgMC0uMTc3LS4wNzNIMi43NWEuMjUuMjUgMCAwIDAtLjI1LjI1Wk02IDVhMSAxIDAgMSAxIDAgMiAxIDEgMCAwIDEgMC0yWiI%2BPC9wYXRoPjwvc3ZnPg%3D%3D&label=Release)](../../releases/latest)
![Game Version](https://img.shields.io/badge/Nuclear_Option-v0.34.2-magenta?style=flat&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciCiAgICAgeG1sbnM6eGxpbms9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGxpbmsiCiAgICAgd2lkdGg9IjI1NiIKICAgICBoZWlnaHQ9IjI1NiIKICAgICB2aWV3Qm94PSIwIDAgMjU2IDI1NiIKICAgICByb2xlPSJpbWciCiAgICAgYXJpYS1sYWJlbGxlZGJ5PSJ0aXRsZSBkZXNjcmlwdGlvbiIKICAgICBzaGFwZS1yZW5kZXJpbmc9Imdlb21ldHJpY1ByZWNpc2lvbiI%2BCiAgPHRpdGxlIGlkPSJ0aXRsZSI%2BTnVjbGVhciBPcHRpb24gbG9nbzwvdGl0bGU%2BCiAgPGRlc2MgaWQ9ImRlc2NyaXB0aW9uIj5BIHNoYXJwIGdlb21ldHJpYyBOIHN1cnJvdW5kaW5nIGEgcmFkaWF0aW9uIHN5bWJvbC48L2Rlc2M%2BCgogIDxkZWZzPgogICAgPHBhdGggaWQ9Im91dGVyLWhhbGYiIGQ9IgogICAgICBNIDE2LDE2CiAgICAgIEwgNzIsNjMKICAgICAgTCA1NCw4NAogICAgICBMIDQ0LDc4CiAgICAgIEwgNDQsMjEyCiAgICAgIEwgMTYsMjQxCiAgICAgIEwgMTYsMTg0CiAgICAgIEwgMjQsMTc1CiAgICAgIEwgMjQsODIKICAgICAgTCAxNiw3MgogICAgICBaIi8%2BCgogICAgPHBhdGggaWQ9InJhZGlhdGlvbi1ibGFkZSIgZD0iCiAgICAgIE0gOTcuNSw3NS4xNwogICAgICBBIDYxLDYxIDAgMCAxIDE1OC41LDc1LjE3CiAgICAgIEwgMTM4LDExMC42OAogICAgICBBIDIwLDIwIDAgMCAwIDExOCwxMTAuNjgKICAgICAgWiIvPgogIDwvZGVmcz4KCiAgPGcgZmlsbD0iI2ZmZmZmZiI%2BCiAgICA8dXNlIHhsaW5rOmhyZWY9IiNvdXRlci1oYWxmIi8%2BCiAgICA8dXNlIHhsaW5rOmhyZWY9IiNvdXRlci1oYWxmIiB0cmFuc2Zvcm09InJvdGF0ZSgxODAgMTI4IDEyOCkiLz4KCiAgICA8Y2lyY2xlIGN4PSIxMjgiIGN5PSIxMjgiIHI9Ijc0IiBmaWxsPSJub25lIiBzdHJva2U9IiNmZmZmZmYiIHN0cm9rZS13aWR0aD0iMTAiLz4KCiAgICA8dXNlIHhsaW5rOmhyZWY9IiNyYWRpYXRpb24tYmxhZGUiLz4KICAgIDx1c2UgeGxpbms6aHJlZj0iI3JhZGlhdGlvbi1ibGFkZSIgdHJhbnNmb3JtPSJyb3RhdGUoMTIwIDEyOCAxMjgpIi8%2BCiAgICA8dXNlIHhsaW5rOmhyZWY9IiNyYWRpYXRpb24tYmxhZGUiIHRyYW5zZm9ybT0icm90YXRlKDI0MCAxMjggMTI4KSIvPgoKICAgIDxjaXJjbGUgY3g9IjEyOCIgY3k9IjEyOCIgcj0iMTYiLz4KICA8L2c%2BCjwvc3ZnPgo%3D&color=007EC6)
![BepInEx Version](https://img.shields.io/badge/BepInEx-v5.4.23.4-magenta?style=flat&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNTYgMjU2Ij48cGF0aCBmaWxsPSIjZmZmIiBmaWxsLXJ1bGU9ImV2ZW5vZGQiIGQ9Ik05NiA2MHYxMTJIMzJ2LTMyaDMycTE1LTEgMTYtMTZWOTJxLTEtMTUtMTYtMTZIMzJWNDRoNDBzOSAxIDgtOGMxLTctOC04LTgtOEgzMnEtMTUgMS0xNiAxNnYzMnExIDE1IDE2IDE2aDMydjMySDMybC01IDFxLTExIDQtMTEgMTV2MzJxMSAxNSAxNiAxNmg2NHExNS0xIDE2LTE2di02NHExLTE1IDE2LTE2IDE1IDEgMTYgMTZ2NjRxMSAxNSAxNiAxNmg2NHExNS0xIDE2LTE2di0zMnEtMS0xNS0xNi0xNmgtMzJWOTJoMzJxMTUtMSAxNi0xNlY0NHEtMS0xNS0xNi0xNmgtNDBzLTggMS04IDhjMCA5IDggOCA4IDhoNDB2MzJoLTMycS0xNSAxLTE2IDE2djMycTEgMTUgMTYgMTZoMzJ2MzJoLTY0VjYxYzAtMTgtMTQtMzMtMzEtMzNoLTFjLTE4IDAtMzIgMTQtMzIgMzJtOC00YTggOCAwIDEgMSAxNiAwIDggOCAwIDEgMS0xNiAwbTMyIDBhOCA4IDAgMSAxIDE2IDAgOCA4IDAgMSAxLTE2IDBtLTE0IDE0YTYgNiAwIDEgMSAxMiAwIDYgNiAwIDEgMS0xMiAwbTggMTMzdjMzcTAgMyA0IDMgMyAwIDQtM3YtMzNxLTEtMy00LTMtNCAwLTQgM20tMTA2LTJxLTUgMC01IDV2MjdxMCA1IDUgNWgxNHExNSAwIDE0LTEyIDAtNi01LTkgMy0xIDMtNiAwLTEwLTEzLTEwem0zIDhoMTBxNiAwIDUgMiAwIDMtNCAzSDI3em0wIDEzaDExcTYgMCA2IDR0LTYgNEgyN3ptMTI3LTEwcS0xLTQtNC00dC0zIDR2MjNxMSAzIDQgM3QzLTN2LTEycTEtNyA3LTcgNCAwIDQgNXYxNHExIDMgNCAzIDQgMCA0LTN2LTE0cTAtMTMtMTItMTMtNSAwLTcgNG0tNTMtMWMwLTMtOC0zLTggMHYzMnExIDMgNCAzIDUgMCA1LTN2LTZsNiAycTEzLTEgMTMtMTUgMC0xNi0xMy0xNi01IDAtNyAzbTAgMTNxMS04IDYtOCA2IDAgNiA4IDAgNy02IDctNS0xLTYtN20tNDMgMHExIDE0IDE0IDE1IDEyIDAgMTMtNmwxLTFxLTEtMy01LTRsLTIgMXEtMSAzLTYgMy02IDAtNy01aDE2cTUgMCA1LTUtMi0xMy0xNC0xMy0xNCAxLTE1IDE1bTE1LThxNSAxIDYgNEg2NnExLTMgNy00bTExMy0xNXEtNSAwLTUgNXYyN3EwIDUgNSA1aDIwYzUgMCA1LTggMC04aC0xN3YtN2gxNXEzIDAgNC00IDAtMy00LTRoLTE1di02aDE3cTQgMCA0LTQgMC0zLTQtNHptNDUgMTAtNCA2LTQtNi0zLTJxLTQgMC01IDRsMSAyIDYgOC02IDEwLTEgMnExIDMgNCA0bDMtMiA1LTcgNSA3IDMgMnE0LTEgNS00bC0xLTItNy0xMCA2LTggMS0ycTAtNC00LTR6Ii8%2BPC9zdmc%2B&color=007EC6)

</div>

<div align="right">

![Downloads](https://img.shields.io/github/downloads/SolarDyn/Solar-UI/total?style=flat&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxNiAxNiIgd2lkdGg9IjE2IiBoZWlnaHQ9IjE2IiBmaWxsPSIjZmZmZmZmIj48cGF0aCBkPSJNMi43NSAxNEExLjc1IDEuNzUgMCAwIDEgMSAxMi4yNXYtMi41YS43NS43NSAwIDAgMSAxLjUgMHYyLjVjMCAuMTM4LjExMi4yNS4yNS4yNWgxMC41YS4yNS4yNSAwIDAgMCAuMjUtLjI1di0yLjVhLjc1Ljc1IDAgMCAxIDEuNSAwdjIuNUExLjc1IDEuNzUgMCAwIDEgMTMuMjUgMTRaIj48L3BhdGg%2BPHBhdGggZD0iTTcuMjUgNy42ODlWMmEuNzUuNzUgMCAwIDEgMS41IDB2NS42ODlsMS45Ny0xLjk2OWEuNzQ5Ljc0OSAwIDEgMSAxLjA2IDEuMDZsLTMuMjUgMy4yNWEuNzQ5Ljc0OSAwIDAgMS0xLjA2IDBMNC4yMiA2Ljc4YS43NDkuNzQ5IDAgMSAxIDEuMDYtMS4wNmwxLjk3IDEuOTY5WiI%2BPC9wYXRoPjwvc3ZnPg%3D%3D&label=Downloads&color=c64800)
![Download Size](https://img.shields.io/badge/Download_Size-8.52_KB-magenta?style=flat&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxNiAxNiIgd2lkdGg9IjE2IiBoZWlnaHQ9IjE2IiBmaWxsPSIjZmZmZmZmIj48cGF0aCBkPSJNMy41IDEuNzV2MTEuNWMwIC4wOS4wNDguMTczLjEyNi4yMTdhLjc1Ljc1IDAgMCAxLS43NTIgMS4yOThBMS43NDggMS43NDggMCAwIDEgMiAxMy4yNVYxLjc1QzIgLjc4NCAyLjc4NCAwIDMuNzUgMGg1LjU4NmMuNDY0IDAgLjkwOS4xODUgMS4yMzcuNTEzbDIuOTE0IDIuOTE0Yy4zMjkuMzI4LjUxMy43NzMuNTEzIDEuMjM3djguNTg2QTEuNzUgMS43NSAwIDAgMSAxMi4yNSAxNWgtLjVhLjc1Ljc1IDAgMCAxIDAtMS41aC41YS4yNS4yNSAwIDAgMCAuMjUtLjI1VjQuNjY0YS4yNS4yNSAwIDAgMC0uMDczLS4xNzdMOS41MTMgMS41NzNhLjI1LjI1IDAgMCAwLS4xNzctLjA3M0g3LjI1YS43NS43NSAwIDAgMSAwIDEuNWgtLjVhLjc1Ljc1IDAgMCAxIDAtMS41aC0zYS4yNS4yNSAwIDAgMC0uMjUuMjVabTMuNzUgOC43NWguNWMuOTY2IDAgMS43NS43ODQgMS43NSAxLjc1djNhLjc1Ljc1IDAgMCAxLS43NS43NWgtMi41YS43NS43NSAwIDAgMS0uNzUtLjc1di0zYzAtLjk2Ni43ODQtMS43NSAxLjc1LTEuNzVaTTYgNS4yNWEuNzUuNzUgMCAwIDEgLjc1LS43NWguNWEuNzUuNzUgMCAwIDEgMCAxLjVoLS41QS43NS43NSAwIDAgMSA2IDUuMjVabS43NSAyLjI1aC41YS43NS43NSAwIDAgMSAwIDEuNWgtLjVhLjc1Ljc1IDAgMCAxIDAtMS41Wk04IDYuNzVBLjc1Ljc1IDAgMCAxIDguNzUgNmguNWEuNzUuNzUgMCAwIDEgMCAxLjVoLS41QS43NS43NSAwIDAgMSA4IDYuNzVaTTguNzUgM2guNWEuNzUuNzUgMCAwIDEgMCAxLjVoLS41YS43NS43NSAwIDAgMSAwLTEuNVpNOCA5Ljc1QS43NS43NSAwIDAgMSA4Ljc1IDloLjVhLjc1Ljc1IDAgMCAxIDAgMS41aC0uNUEuNzUuNzUgMCAwIDEgOCA5Ljc1Wm0tMSAyLjV2Mi4yNWgxdi0yLjI1YS4yNS4yNSAwIDAgMC0uMjUtLjI1aC0uNWEuMjUuMjUgMCAwIDAtLjI1LjI1WiI%2BPC9wYXRoPjwvc3ZnPg%3D%3D&color=c64800)
![Assembly Size](https://img.shields.io/badge/Assembly_Size-21.00_KB-magenta?style=flat&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxNiAxNiIgd2lkdGg9IjE2IiBoZWlnaHQ9IjE2IiBmaWxsPSIjZmZmZmZmIj48cGF0aCBkPSJNNCAxLjc1QzQgLjc4NCA0Ljc4NCAwIDUuNzUgMGg1LjU4NmMuNDY0IDAgLjkwOS4xODQgMS4yMzcuNTEzbDIuOTE0IDIuOTE0Yy4zMjkuMzI4LjUxMy43NzMuNTEzIDEuMjM3djguNTg2QTEuNzUgMS43NSAwIDAgMSAxNC4yNSAxNWgtOWEuNzUuNzUgMCAwIDEgMC0xLjVoOWEuMjUuMjUgMCAwIDAgLjI1LS4yNVY2aC0yLjc1QTEuNzUgMS43NSAwIDAgMSAxMCA0LjI1VjEuNUg1Ljc1YS4yNS4yNSAwIDAgMC0uMjUuMjV2MmEuNzUuNzUgMCAwIDEtMS41IDBabS00IDZDMCA2Ljc4NC43ODQgNiAxLjc1IDZoMS41QzQuMjE2IDYgNSA2Ljc4NCA1IDcuNzV2Mi41QTEuNzUgMS43NSAwIDAgMSAzLjI1IDEyaC0xLjVBMS43NSAxLjc1IDAgMCAxIDAgMTAuMjVaTTYuNzUgNmgxLjVhLjc1Ljc1IDAgMCAxIC43NS43NXYzLjc1aC43NWEuNzUuNzUgMCAwIDEgMCAxLjVoLTNhLjc1Ljc1IDAgMCAxIDAtMS41aC43NXYtM2gtLjc1YS43NS43NSAwIDAgMSAwLTEuNVptLTUgMS41YS4yNS4yNSAwIDAgMC0uMjUuMjV2Mi41YzAgLjEzOC4xMTIuMjUuMjUuMjVoMS41YS4yNS4yNSAwIDAgMCAuMjUtLjI1di0yLjVhLjI1LjI1IDAgMCAwLS4yNS0uMjVabTkuNzUtNS45MzhWNC4yNWMwIC4xMzguMTEyLjI1LjI1LjI1aDIuNjg4bC0uMDExLS4wMTMtMi45MTQtMi45MTQtLjAxMy0uMDExWiI%2BPC9wYXRoPjwvc3ZnPg%3D%3D&color=c64800)

</div>

**Utilities & Integrations** is a library for easier updating and reducing duplicate code across dozens of mods.

<div align="center">
  <img src="https://github.com/SolarDyn/Assets/blob/main/background/ifrit.png" />
</div>

* [Features](#features)
* [Planned](#planned)
* [Social](#social)
* [Requirements](#requirements)
* [Installation](#installation)
* [Compatibility](#compatibility)
* [Known Issues](#known-issues)
* [Bug Reports](#bug-reports)
  * [Default Windows Paths](#default-windows-paths)
  * [Default GNU/Linux Paths](#default-gnulinux-paths)
* [Credits](#credits)
* [Disclaimer](#disclaimer)

## Features

- Collimated HUD compatible UI widgets.

## Planned

- Logging utilities.
- Importing and modifying assets.
- A lot of other shit...

## Social

[![GitHub Stars](https://img.shields.io/github/stars/SolarDyn?style=flat&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxNiAxNiIgd2lkdGg9IjE2IiBoZWlnaHQ9IjE2IiBmaWxsPSIjZmZmZmZmIj48cGF0aCBkPSJNOCAuMjVhLjc1Ljc1IDAgMCAxIC42NzMuNDE4bDEuODgyIDMuODE1IDQuMjEuNjEyYS43NS43NSAwIDAgMSAuNDE2IDEuMjc5bC0zLjA0NiAyLjk3LjcxOSA0LjE5MmEuNzUxLjc1MSAwIDAgMS0xLjA4OC43OTFMOCAxMi4zNDdsLTMuNzY2IDEuOThhLjc1Ljc1IDAgMCAxLTEuMDg4LS43OWwuNzItNC4xOTRMLjgxOCA2LjM3NGEuNzUuNzUgMCAwIDEgLjQxNi0xLjI4bDQuMjEtLjYxMUw3LjMyNy42NjhBLjc1Ljc1IDAgMCAxIDggLjI1Wm0wIDIuNDQ1TDYuNjE1IDUuNWEuNzUuNzUgMCAwIDEtLjU2NC40MWwtMy4wOTcuNDUgMi4yNCAyLjE4NGEuNzUuNzUgMCAwIDEgLjIxNi42NjRsLS41MjggMy4wODQgMi43NjktMS40NTZhLjc1Ljc1IDAgMCAxIC42OTggMGwyLjc3IDEuNDU2LS41My0zLjA4NGEuNzUuNzUgMCAwIDEgLjIxNi0uNjY0bDIuMjQtMi4xODMtMy4wOTYtLjQ1YS43NS43NSAwIDAgMS0uNTY0LS40MUw4IDIuNjk0WiI%2BPC9wYXRoPjwvc3ZnPg%3D%3D&label=Stars&labelColor=%2324292F&color=%236E40C9)](https://github.com/orgs/SolarDyn/repositories)
[![GitHub Followers](https://img.shields.io/github/followers/SolarDyn?style=flat&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAxNiAxNiIgd2lkdGg9IjE2IiBoZWlnaHQ9IjE2IiBmaWxsPSIjZmZmZmZmIj48cGF0aCBkPSJNOCAxNmEyIDIgMCAwIDAgMS45ODUtMS43NWMuMDE3LS4xMzctLjA5Ny0uMjUtLjIzNS0uMjVoLTMuNWMtLjEzOCAwLS4yNTIuMTEzLS4yMzUuMjVBMiAyIDAgMCAwIDggMTZaTTMgNWE1IDUgMCAwIDEgMTAgMHYyLjk0N2MwIC4wNS4wMTUuMDk4LjA0Mi4xMzlsMS43MDMgMi41NTVBMS41MTkgMS41MTkgMCAwIDEgMTMuNDgyIDEzSDIuNTE4YTEuNTE2IDEuNTE2IDAgMCAxLTEuMjYzLTIuMzZsMS43MDMtMi41NTRBLjI1NS4yNTUgMCAwIDAgMyA3Ljk0N1ptNS0zLjVBMy41IDMuNSAwIDAgMCA0LjUgNXYyLjk0N2MwIC4zNDYtLjEwMi42ODMtLjI5NC45N2wtMS43MDMgMi41NTZhLjAxNy4wMTcgMCAwIDAtLjAwMy4wMWwuMDAxLjAwNmMwIC4wMDIuMDAyLjAwNC4wMDQuMDA2bC4wMDYuMDA0LjAwNy4wMDFoMTAuOTY0bC4wMDctLjAwMS4wMDYtLjAwNC4wMDQtLjAwNi4wMDEtLjAwN2EuMDE3LjAxNyAwIDAgMC0uMDAzLS4wMWwtMS43MDMtMi41NTRhMS43NDUgMS43NDUgMCAwIDEtLjI5NC0uOTdWNUEzLjUgMy41IDAgMCAwIDggMS41WiI%2BPC9wYXRoPjwvc3ZnPg%3D%3D&label=Followers&labelColor=%2324292F&color=%236E40C9)](https://github.com/SolarDyn)

[![Discord](https://img.shields.io/discord/1047145310654300180?style=flat&logo=discord&logoColor=white&label=Solar%20Development&labelColor=%2324292F&color=%232DA44E)](https://discord.gg/S6rzTDpZYB)

<div align="center">
  <img src="https://github.com/SolarDyn/Assets/blob/main/background/vagrant.png" />
</div>

## Requirements

* Nuclear Option: v0.34.2
* BepInEx: v5.4.23.4

## Installation

### Automatic

1. Install [NOMM](https://github.com/Combat787/NOMM).
2. Use NOMM to install all the mods you want.

### Manual

1. Install [BepInEx](https://github.com/BepInEx/BepInEx).
2. Download the latest release from
   the [Releases](https://github.com/SolarDyn/Solar-UI/releases) page.
3. Extract the contents into your plugins folder:

```terminaloutput
Nuclear Option/
└── BepInEx/
    └── plugins/
        └── Solar.UI/
            └── Solar.UI.dll
```

## Compatibility

This project modifies the following game systems:

- `I'll worry about this later.`

Mods that patch the same methods may conflict.

## Known Issues

- None.

If you encounter a bug not listed here, please [report it](#bug-reports).

## Bug Reports

Bug reports should be submitted through
the [GitHub Issues](https://github.com/SolarDyn/Solar-UI/issues) page.

> [!IMPORTANT]
> When reporting a problem, include as much of the following relevant information as possible:
> 
> * Versions:
>     * Mod version.
>     * Nuclear Option version.
>     * BepInEx version.
> * Logs:
>     * Nuclear Option log.
>     * BepInEx log.
> * Steps to reproduce the problem.
> * Other installed mods that may be relevant.

### Default Windows Paths

Nuclear Option Log:

```text
%LOCALAPPDATA%Low\Shockfront\NuclearOption\Player.log
```

BepInEx Log:

```text
%PROGRAMFILES(X86)%\Steam\steamapps\common\Nuclear Option\BepInEx\LogOutput.log
```

### Default GNU/Linux Paths

Nuclear Option Log:

```text
~/.local/share/Steam/steamapps/compatdata/2168680/pfx/drive_c/users/steamuser/AppData/LocalLow/Shockfront/NuclearOption/Player.log
```

BepInEx Log:

```text
~/.local/share/Steam/steamapps/common/Nuclear Option/BepInEx/LogOutput.log
```

## Credits

| Resource       | Author             |
|----------------|--------------------|
| Nuclear Option | Shockfront Studios |
| MonoMod        | 0x0ade             |
| HarmonyX       | BepInEx            |
| BepInEx        | BepInEx            |

## Disclaimer

This project is an unofficial modification for *Nuclear Option* and is not affiliated with or endorsed by Shockfront
Studios.
