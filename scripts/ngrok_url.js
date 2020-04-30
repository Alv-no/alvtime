const fetch = require("node-fetch");
const fs = require("fs");

(async () => {
  const respons = await fetch("http://localhost:4040/api/tunnels");
  const jsonRes = await respons.json();
  const httpTunnel = jsonRes.tunnels.find((tunnel) => tunnel.proto === "http");
  const publicUrl = httpTunnel.public_url;

  const filePath = process.cwd() + "/.env";
  fs.readFile(filePath, "utf8", function (err, data) {
    if (err) {
      return console.log(err);
    }

    const result = data.replace(
      /HOST=.+/,
      "HOST=" + publicUrl
    );

    fs.writeFile(filePath, result, "utf8", function (err) {
      if (err) return console.log(err);
    });
  });
})();
