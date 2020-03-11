import { State, Task } from "./index";
import { ActionContext } from "vuex";
import config from "@/config";
import { adAuthenticatedFetch } from "@/services/auth";

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

    UPDATE_TASKS(state: State, paramTasks: Task[]) {
      for (const paramTask of paramTasks) {
        state.tasks = [
          ...state.tasks.map((task: Task) =>
            task.id === paramTask.id ? paramTask : task
          ),
        ];
      }
    },
  },

  actions: {
    FETCH_TASKS: async ({ commit }: ActionContext<State, State>) => {
      const url = new URL(config.HOST + "/api/user/tasks").toString();
      const res = await adAuthenticatedFetch(url);
      const tasks = await res.json();
      commit("SET_TASKS", tasks);
    },

    PUSH_TASKS: async (
      { state, commit }: ActionContext<State, State>,
      paramTasks: Task[]
    ) => {
      try {
        const method = "post";
        const headers = { "Content-Type": "application/json" };
        const body = JSON.stringify(paramTasks);
        const options = { method, headers, body };

        const response = await adAuthenticatedFetch(
          config.HOST + "/api/user/Tasks",
          options
        );
        const updatedTasks = await response.json();
        if (response.status !== 200) {
          throw Error(`${response.statusText}
            ${updatedTasks.title}`);
        }
        commit("UPDATE_TASKS", updatedTasks);
      } catch (e) {
        console.error(e);
      }
    },
  },
};
