import env from "../../environment";

export default function createLoginPage(slackTeamDomain: string) {
  return `
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <title></title>
    <style>
      body {
        display: grid;
        justify-content: center;
        background-color: #f7f7f7;
        font-family: "Source Sans Pro", sans-serif;
        font-weight: 400;
        line-height: 1.5;
        color: #312f30;
      }

      .container {
        margin-top: 3rem;
        padding: 2rem;
        border-radius: 30px;
        text-align: center;
      }

      .yellow-button {
        display: inline-block;
        background-color: transparent;
        -webkit-border-radius: 30px;
        -moz-border-radius: 30px;
        -ms-border-radius: 30px;
        border-radius: 30px;
        -khtml-border-radius: 30px;
        padding: 10px 40px;
        border: 2px solid #e8b925;
        line-height: 1rem;
        font-family: "Source Sans Pro", sans-serif;
        font-weight: 600;
        font-size: 14px;
        white-space: nowrap;
        cursor: pointer;
        -webkit-transition: all 300ms linear;
        -moz-transition: all 300ms linear;
        -ms-transition: all 300ms linear;
        -o-transition: all 300ms linear;
        transition: all 300ms linear;
        -webkit-touch-callout: none;
        -webkit-user-select: none;
        -khtml-user-select: none;
        -moz-user-select: none;
        -ms-user-select: none;
        user-select: none;
        text-transform: uppercase;
        color: black;
        text-decoration: none;
      }

      .yellow-button:hover {
        background-color: #eabb26;
      }

      .workspace-line {
        margin-bottom: 2rem;
      }

      .workspace {
        font-size: 26px;
        font-weight: 300;
      }
    </style>
  </head>
  <body>
    <body>
      <div class="container">
        <div
          class="container-lg p-responsive py-6 py-lg-10"
          style="max-width: 600px;"
        >
          <img
            src="/images/alvtimeplusslack.svg"
            class="alvtimeplusslack"
            style="max-width: 300px;"
          />
          <p>Koble din Alvtime konto til din Slack konto</p>
          <p class="workspace-line">
            for
            <span class="workspace">${slackTeamDomain}.slack.com</span>
            arbeidsgruppen.
          </p>
          <p>
            Dette vil koble kontoene sammen slik at du kan bruke Slack til å se uttrekk av førte timer, føre og fjerne timer og få tilpassede påminnelser om å føre timer. Dette og mye mer som er avhengig av din tilgang til Alvtime.
          </p>
          <p></p>
          <a
            href="${env.HOST}/oauth2/azureAd"
            class="yellow-button"
            >Koble til Alvtime konto</a
          >
        </div>
      </div>
    </body>
  </body>
</html>
  `;
}
