import Vue from "vue";
import VueRouter from "vue-router";
import Hours from "../views/Hours.vue";
import Tasks from "../views/Tasks.vue";
import AccumulatedHours from "../views/AccumulatedHours.vue";
import Tokens from "../views/Tokens.vue";
import UnAutherized from "../views/UnAutherized.vue";
import Login from "../views/Login.vue";
import Summarizedhours from "../views/Summarizedhours.vue";
import store from "@/store";
import authService from "@/services/auth";

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
    path: "/summarizedhours",
    name: "summarizedhours",
    component: Summarizedhours,
  },
];

const router = new VueRouter({
  routes,
});

router.beforeEach(async (to, _from, next) => {
  if (to.name === "UnAutherized" || to.name === "login") {
    next();
  } else if (await authService.requireLogin()) {
    next("login");
  } else if (!store.state.tasks.length) {
    await store.dispatch("FETCH_USER_DETAILS");
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
