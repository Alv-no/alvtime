import click


@click.command()
@click.argument("shell", type=click.Choice(['bash', 'zsh']))
def completion(shell):
    if shell == "bash":
        click.secho("Add this to ~/.bashrc:", fg="green")
        click.secho('eval "$(_ALVTIME_COMPLETE=bash_source alvtime)"', fg="white", bold=True)
    elif shell == "zsh":
        click.secho("Add this to ~/.zshrc:", fg="green")
        click.echo()
        click.secho('eval "$(_ALVTIME_COMPLETE=zsh_source alvtime)"', fg="white", bold=True)
