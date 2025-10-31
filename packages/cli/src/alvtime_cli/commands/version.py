import click
from importlib.metadata import version, PackageNotFoundError


@click.command(name="version", help="Displays alvtime cli version")
def version_():
    try:
        package_version = version("alvtime-cli")
        print(package_version)
    except PackageNotFoundError:
        print("Package not installed, version not found.")
