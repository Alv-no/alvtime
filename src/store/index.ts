import Vue from "vue";
import Vuex from "vuex";

Vue.use(Vuex);

export default new Vuex.Store({
  state: {
    tasks: [
      {
        id: 1,
        name: "Alvtime frontend",
        description: "Code up some Alvtime frontend",
        projectId: 1,
        projectName: "Alvtime",
        customerId: 1,
        customerName: "Alv",
        hourRate: 0,
        favorite: true,
        locked: false,
      },
      {
        id: 2,
        name: "Bordtennis",
        description: "Vinne",
        projectId: 2,
        projectName: "Sosial trening",
        customerId: 1,
        customerName: "Alv",
        hourRate: 0,
        favorite: true,
        locked: false,
      },
      {
        id: 3,
        name: "Ferie",
        description: "Null niks nada",
        projectId: 3,
        projectName: "Fritid",
        customerId: 1,
        customerName: "Alv",
        hourRate: 0,
        favorite: true,
        locked: false,
      },
      {
        id: 4,
        name: "Manuelle endringer i database",
        description: "Fullstending It revolusjon",
        projectId: 4,
        projectName: "It revolusjon",
        customerId: 2,
        customerName: "Kunde AS",
        hourRate: 0,
        favorite: true,
        locked: false,
      },
    ],
    timeEntries: [
      {
        id: 1,
        date: "2020-01-03",
        value: 7.5,
        taskId: 4,
      },
      {
        id: 2,
        date: "2020-01-04",
        value: 7.5,
        taskId: 4,
      },
      {
        id: 3,
        date: "2020-01-05",
        value: 7.5,
        taskId: 4,
      },
      {
        id: 4,
        date: "2020-01-06",
        value: 7.5,
        taskId: 4,
      },
    ],
  },
  getters: {},
  mutations: {},
  actions: {},
  modules: {},
});
