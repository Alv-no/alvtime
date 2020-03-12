import Vue from "vue";
import VueRouter, { Route, RawLocation } from "vue-router";
import Home from "../views/Home.vue";
import Login from "../views/Login.vue";
import store from "../store";
import { getAccount } from "../services/auth";

Vue.use(VueRouter);

const routes = [
  {
    path: "*",
    name: "home",
    component: Home,
    // beforeEnter(
    //   to: Route,
    //   from: Route,
    //   next: (to?: RawLocation | false | ((vm: Vue) => any) | void) => void
    // ) {
    //   // The following dispatches call the API
    //   // All API calls run the acquireTokenSilent function
    //   // acquireTokenSilent navigates through an iframe.
    //   // To not run acquireTokenSilent in iframes isInIframe is ran
    //   next();
    // },
  },
];

const router = new VueRouter({
  routes,
});

// router.beforeEach(
//   (
//     to: Route,
//     from: Route,
//     next: (to?: RawLocation | false | ((vm: Vue) => any) | void) => void
//   ) => {
//     const isAuthenticated = getAccount();
//     console.log("from: ", from);
//     console.log("to: ", to);
//     console.log("isAuthenticated: ", isAuthenticated);
//     if (to.name !== "login" && !isAuthenticated) {
//       next({ name: "login" });
//     } else if (to.name !== "home" && isAuthenticated) {
//       next({ name: "home" });
//     } else {
//       next();
//     }
//   }
// );

export default router;
