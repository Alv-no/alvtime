import { State } from "./index";
import { ActionContext } from "vuex";
import config from "@/config";
import httpClient from "../services/httpClient";

export interface TaskState {
  tasks: Task[];
}
export interface Task {
  id: number;
  compensationRate: number;
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
    httpClient.get(`${config.API_HOST}/api/user/tasks`).then(response => {
      commit("SET_TASKS", response.data);
    });
  },

  PUSH_TASKS: async (
    { commit }: ActionContext<State, State>,
    paramTasks: Task[]
  ) => {
    httpClient
      .post(`${config.API_HOST}/api/user/Tasks`, paramTasks)
      .then(response => {
        commit("UPDATE_TASKS", response.data);
      });
  },
};

export default {
  state,
  getters,
  mutations,
  actions,
};
