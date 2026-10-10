#!/usr/bin/env python3
"""Check the official NuGet index without treating service failures as absence."""

import argparse
import json
import re
import socket
import ssl
import sys
import time
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen


ATTEMPTS = 3
TIMEOUT_SECONDS = 15
PACKAGE_ID = re.compile(r"[A-Za-z0-9][A-Za-z0-9_.-]{0,99}\Z")
VERSION = re.compile(
    r"[0-9]+(?:\.[0-9]+){2,3}"
    r"(?:-[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)*)?\Z"
)


def is_temporary_network_error(error):
    reason = error.reason if isinstance(error, URLError) else error
    if isinstance(reason, ssl.SSLCertVerificationError):
        return False
    if isinstance(reason, socket.gaierror):
        return reason.errno == socket.EAI_AGAIN
    return isinstance(reason, (TimeoutError, ConnectionError, OSError))


def version_exists(package_id, version):
    if not PACKAGE_ID.fullmatch(package_id):
        raise ValueError("Invalid package ID: use 1-100 ASCII letters, digits, '.', '_' or '-'.")
    if len(version) > 256 or not VERSION.fullmatch(version):
        raise ValueError("Invalid version: use three or four numeric components and an optional prerelease suffix.")

    request = Request(
        f"https://api.nuget.org/v3-flatcontainer/{package_id.lower()}/index.json",
        headers={"Accept": "application/json", "User-Agent": "Janglim-release-check"},
    )
    for attempt in range(ATTEMPTS):
        try:
            with urlopen(request, timeout=TIMEOUT_SECONDS) as response:
                data = json.load(response)
            if (
                not isinstance(data, dict)
                or not isinstance(data.get("versions"), list)
                or any(not isinstance(item, str) for item in data["versions"])
            ):
                raise RuntimeError("NuGet returned an invalid version index.")
            return version.lower() in (item.lower() for item in data["versions"])
        except HTTPError as error:
            if error.code == 404:
                return False
            if error.code != 429 and not 500 <= error.code <= 599:
                raise RuntimeError(f"NuGet returned HTTP {error.code}.") from error
        except (URLError, OSError) as error:
            if not is_temporary_network_error(error):
                raise RuntimeError("NuGet connection failed with a non-retryable network error.") from error
        except (json.JSONDecodeError, UnicodeError) as error:
            raise RuntimeError("NuGet returned malformed JSON.") from error

        if attempt + 1 == ATTEMPTS:
            raise RuntimeError(f"NuGet version check failed after {ATTEMPTS} attempts.")
        time.sleep(attempt + 1)


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("package_id")
    parser.add_argument("version")
    args = parser.parse_args(argv)
    try:
        exists = version_exists(args.package_id, args.version)
    except (ValueError, RuntimeError) as error:
        print(f"Error: {error}", file=sys.stderr)
        return 1
    print(f"exists={str(exists).lower()}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
