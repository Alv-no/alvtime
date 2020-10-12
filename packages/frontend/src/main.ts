import Vue from "vue";
import App from "./App.vue";
import "swiper/css/swiper.css";
import "./registerServiceWorker";
import router from "./router";
import store from "./store";
import "vue-material/dist/vue-material.min.css";
import "vue-material/dist/theme/default.css";
import {
  MdButton,
  MdSnackbar,
  MdIcon,
  MdCheckbox,
  MdTooltip,
  MdProgress,
  MdApp,
  MdToolbar,
  MdContent,
  MdDrawer,
  MdList,
  MdAvatar,
  MdRipple,
  MdEmptyState,
  MdDialog,
  MdDialogConfirm,
  MdDatepicker,
} from "vue-material/dist/components";
import VueMq from "vue-mq";
import VueClipboard from "vue-clipboard2";

Vue.use(VueMq);
Vue.use(MdButton);
Vue.use(MdSnackbar);
Vue.use(MdIcon);
Vue.use(MdCheckbox);
Vue.use(MdTooltip);
Vue.use(MdProgress);
Vue.use(MdApp);
Vue.use(MdToolbar);
Vue.use(MdContent);
Vue.use(MdDrawer);
Vue.use(MdList);
Vue.use(MdAvatar);
Vue.use(MdRipple);
Vue.use(MdEmptyState);
Vue.use(MdDialog);
Vue.use(MdDialogConfirm);
Vue.use(MdDatepicker);

Vue.use(VueClipboard);

Vue.config.productionTip = false;

new Vue({
  router,
  store,
  render: h => h(App),
}).$mount("#app");
