module.exports = {
  lintOnSave: false,
  pwa: {
    name: "Alvtime",
    themeColor: "#061838",
    appleMobileWebAppCapable: "yes",
    appleMobileWebAppStatusBarStyle: "black",
    workboxPluginMode: "InjectManifest",
    workboxOptions: {
      swSrc: "./src/sw.js",
      swDest: "service-worker.js",
    },
  },
  devServer: {
    disableHostCheck: true,
  },
};
