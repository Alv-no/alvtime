import Vue from "vue";
import VueRouter, { Route, RawLocation } from "vue-router";
import Home from "../views/Home.vue";
import Login from "../views/Login.vue";
import store from "../store";
import { msalApp } from "../services/auth";

Vue.use(VueRouter);

const routes = [
  {
    path: "/login",
    name: "login",
    component: Login,
  },
  {
    path: "*",
    name: "home",
    component: Home,
    beforeEnter(
      to: Route,
      from: Route,
      next: (to?: RawLocation | false | ((vm: Vue) => any) | void) => void
    ) {
      // The following dispatches call the API
      // All API calls run the msalApp.acquireTokenSilent function
      // msalApp.acquireTokenSilent navigates through an iframe.
      // To not run acquireTokenSilent in iframes isInIframe is ran
      if (!isInIframe()) {
        store.dispatch("FETCH_TASKS");
        store.dispatch("FETCH_TIME_ENTRIES");
      }
      next();
    },
  },
];

const router = new VueRouter({
  routes,
});

router.beforeEach((to, from, next) => {
  const isAuthenticated = msalApp.getAccount();
  if (to.name !== "login" && !isAuthenticated) {
    next({ name: "login" });
  } else {
    next();
  }
});

export default router;

function isInIframe(): boolean {
  return window.parent !== window;
}
