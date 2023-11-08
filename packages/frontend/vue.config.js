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
    allowedHosts: 'all',
  },
  chainWebpack: (config) => {
    config.resolve.alias.set('vue', '@vue/compat')

    config.module
        .rule('vue')
        .use('vue-loader')
        .tap((options) => {
          return {
            ...options,
            compilerOptions: {
              compatConfig: {
                MODE: 2
              }
            }
          }
        })
  }
};
