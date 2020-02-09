import { State, Task } from "./index";
import { ActionContext } from "vuex";
import config from "@/config";

export default {
  state: {
    tasks: [],
  },

  getters: {
    getTask: (state: State) => (id: number) => {
      return state.tasks.find(task => task.id === id);
    },
    favoriteTasks: (state: State) => {
      return state.tasks.filter(task => task.favorite);
    },
    notFavoriteTasks: (state: State) => {
      return state.tasks.filter(task => !task.favorite);
    },
  },

  mutations: {
    SET_TASKS(state: State, paramTasks: Task[]) {
      state.tasks = paramTasks;
    },
  },

  actions: {
    FETCH_TASKS: async ({ commit }: ActionContext<State, State>) => {
      const url = new URL(config.HOST + "/api/user/tasks").toString();
      const res = await fetch(url);
      const tasks = await res.json();
      commit("SET_TASKS", tasks);
    },
  },
};
