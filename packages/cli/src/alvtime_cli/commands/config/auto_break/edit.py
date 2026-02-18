import click
import pydantic
import textwrap
import yaml
from alvtime_cli import config


@click.command()
def edit():
    # Get the current auto-break config
    breaks = config.get("autoBreaks", [])

    # Build the YAML for the user to edit
    template = textwrap.dedent("""
        # - comment: Lunsj
        #   weekdays:
        #   - mon
        #   - tue
        #   - wed
        #   - thu
        #   - fri
        #   start: "11:30"
        #   stop: "12:00"

        """).lstrip()

    if breaks:
        text = yaml.dump(breaks)
    else:
        text = template

    # Let the user edit the YAML in their default editor
    try:
        text_response = click.edit(text, extension=".yaml")
    except click.exceptions.ClickException:
        text_response = None

    # If not cancelled or not saved
    if text_response is None:
        click.echo("Cancelled.", err=True)
        return

    # Parse the YAML
    try:
        untyped_response = yaml.safe_load(text_response)
    except yaml.YAMLError as e:
        click.echo(f"YAML error: {e}", err=True)
        return

    # Treat empty file as empty list
    if untyped_response is None:
        untyped_response = []

    # Ensure the top-level structure is a list
    if not isinstance(untyped_response, list):
        click.echo("The YAML must be a list of auto-break objects.", err=True)
        return

    # Validate against the schema
    try:
        pydantic.parse_obj_as(list[config.AutoBreak], untyped_response)
    except pydantic.ValidationError as e:
        click.echo("Invalid auto-break configuration:", err=True)
        click.echo(e, err=True)
        return

    # Save config
    config.set("autoBreaks", untyped_response)
