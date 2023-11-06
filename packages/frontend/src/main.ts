import Vue from "vue";
import App from "./App.vue";
import "swiper/css/swiper.css";
import "./registerServiceWorker";
import router from "./router";
import store from "./store";

import VueMq from "vue-mq";


// import VueDatePicker from '@vuepic/vue-datepicker';
// import '@vuepic/vue-datepicker/dist/main.css';


Vue.use(VueMq);


Vue.config.productionTip = false;

new Vue({
  router,
  store,
  render: h => h(App),
}).$mount("#app");
