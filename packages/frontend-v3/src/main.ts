import { createApp } from "vue";
import "./assets/scss/style.scss";
import App from "./App.vue";
import { router } from "./router";
import { createPinia } from "pinia";
import { useAuthStore } from "./stores/authStore";
import { register } from "swiper/element/bundle";
// register Swiper custom elements
register();

const pinia = createPinia();
const app = createApp(App);

app.use(pinia);
app.use(router);
app.mount("#app");

const authStore = useAuthStore();
authStore.checkAuth();