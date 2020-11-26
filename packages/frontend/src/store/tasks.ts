import { State } from "./index";
import { ActionContext } from "vuex";
import config from "@/config";
import { adAuthenticatedFetch } from "@/services/auth";

export interface TaskState {
  tasks: Task[];
}
export interface Task {
  id: number;
  name: string;
  description: string;
  hourRate: number;
  project: {
    id: number;
    name: string;
    customer: {
      id: number;
      name: string;
    };
  };
  favorite: boolean;
  locked: boolean;
}

const state = {
  tasks: [],
};

const getters = {
  getTask: (state: State) => (id: number) => {
    return state.tasks.find(task => task.id === id);
  },
  favoriteTasks: (state: State) => {
    return state.tasks.filter(task => task.favorite);
  },
  notFavoriteTasks: (state: State) => {
    return state.tasks.filter(task => !task.favorite);
  },
};

const mutations = {
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
};

const actions = {
  FETCH_TASKS: async ({ commit }: ActionContext<State, State>) => {
    try {
      const url = new URL(config.API_HOST + "/api/user/tasks").toString();
      const res = await adAuthenticatedFetch(url);
      if (res.status === 404) {
        commit("SET_USER_NOT_FOUND");
        throw res.statusText;
      }
      const tasks = await res.json();
      commit("SET_TASKS", tasks);
    } catch (e) {
      if (e !== "Not Found") {
        console.error(e);
        commit("ADD_TO_ERROR_LIST", e);
      }
    }
  },

  PUSH_TASKS: async (
    { commit }: ActionContext<State, State>,
    paramTasks: Task[]
  ) => {
    try {
      const method = "post";
      const headers = { "Content-Type": "application/json" };
      const body = JSON.stringify(paramTasks);
      const options = { method, headers, body };

      const response = await adAuthenticatedFetch(
        config.API_HOST + "/api/user/Tasks",
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
      commit("ADD_TO_ERROR_LIST", e);
    }
  },
};

export default {
  state,
  getters,
  mutations,
  actions,
};
