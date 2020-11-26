import { Options } from "material-table";
import React from "react";
import { mutate } from "swr";
import CustomerTable from "./CustomerTable";
import HourRates from "./HourRatesTable";
import ProjectsTable from "./ProjectsTable";
import TasksTable from "./TasksTable";
import UserTable from "./UserTable";

export const setCache = (path: string, data: any) => mutate(path, data, false);

export const globalTableOptions: Options<object> = {
  addRowPosition: "first",
};

export default function Tables() {
  return (
    <>
      <UserTable />
      <CustomerTable />
      <ProjectsTable />
      <TasksTable />
      <HourRates />
    </>
  );
}
