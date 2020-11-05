export default function () {
  return `
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <title></title>
    <style>
      body,
      html {
        height: 100%;
        margin: 0;
      }

      .bg {
        /* The image used */
        background-image: url("/images/Computers_Blue_Screen_of_Death.jpg");

        /* Full height */
        height: 100%;

        /* Center and scale the image nicely */
        background-position: center;
        background-repeat: no-repeat;
        background-size: cover;
      }
    </style>
  </head>
  <body>
    <body>
      <div class="bg"></div>
    </body>
  </body>
</html>
  `;
}
