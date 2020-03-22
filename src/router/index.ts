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
  },
];

const router = new VueRouter({
  routes,
});

export default router;
