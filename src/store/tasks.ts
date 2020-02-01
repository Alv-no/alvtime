import { State } from "./index";

export default {
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
        favorite: false,
        locked: false,
      },
      {
        id: 5,
        name: "Copy paste fra stack overflow",
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

  actions: {
    FETCH_TASKS: async () => {
      const url = new URL("http://localhost/api/user/tasks").toString();
      const res = await fetch(url);
      const tasks = await res.json();
      console.log("tasks: ", tasks);
    },
  },
};
