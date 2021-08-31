import Vue from "vue";
import VueRouter from "vue-router";
import Hours from "../views/Hours.vue";
import Tasks from "../views/Tasks.vue";
import Dashboard from "../views/Dashboard.vue";
import AccumulatedHours from "../views/AccumulatedHours.vue";
import Tokens from "../views/Tokens.vue";
import UnAutherized from "../views/UnAutherized.vue";
import Login from "../views/Login.vue";
import store from "@/store";
import { requireLogin } from "@/services/auth";

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
    path: "/accumulated-hours",
    name: "accumulated-hours",
    component: AccumulatedHours,
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
  {
    path: "/dashboard",
    name: "dashboard",
    component: Dashboard,
  },
];

const router = new VueRouter({
  routes,
});

router.beforeEach(async (to, _from, next) => {
  if (to.name === "UnAutherized" || to.name === "login") {
    next();
  } else if (requireLogin()) {
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
