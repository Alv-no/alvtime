import Vue from "vue";
import VueRouter from "vue-router";
import Hours from "../views/Hours.vue";
import Tasks from "../views/Tasks.vue";
import Tokens from "../views/Tokens.vue";
import UnAutherized from "../views/UnAutherized.vue";
import Login from "../views/Login.vue";
import store from "@/store";

Vue.use(VueRouter);

const routes = [
  { path: "/", redirect: { name: "hours" } },
  {
    path: "/hours",
    name: "hours",
    component: Hours,
  },
  {
    path: "/tasks",
    name: "tasks",
    component: Tasks,
  },
  {
    path: "/tokens",
    name: "tokens",
    component: Tokens,
  },
  {
    path: "/UnAutherized",
    name: "UnAutherized",
    component: UnAutherized,
  },
  {
    path: "/login",
    name: "login",
    component: Login,
  },
];

const router = new VueRouter({
  routes,
});

router.beforeEach(async (to, _from, next) => {
  if (to.name === "UnAutherized" || to.name === "login") {
    next();
  } else if (!store.state.account) {
    next("login");
  } else if (!store.state.tasks.length) {
    await store.dispatch("FETCH_TASKS");
    if (store.state.userNotFound) {
      next("UnAutherized");
    } else {
      next();
    }
  } else {
    next();
  }
});

router.afterEach(route => {
  store.commit("SET_CURRENT_ROUTE", route);
});

export default router;
