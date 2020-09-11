import React from "react";
import { mutate } from "swr";
import { createAdAuthenticatedFetch } from "../services/azureAd";
import CustomerTable from "./CustomerTable";
import UserTable from "./UserTable";
import TasksTable from "./TasksTable";
import ProjectsTable from "./ProjectsTable";
import AssociatedTasksTable from "./AssociatedTasksTable";
import { Options } from "material-table";

export const fetcher = async (path: string, paramOptions: any) => {
  const contentTypeHeader = {
    "Content-Type": "application/json",
  };
  paramOptions = paramOptions ? paramOptions : { headers: {} };

  if (paramOptions.body) {
    paramOptions.body = JSON.stringify(paramOptions.body);
  }

  var options = {
    ...paramOptions,
    headers: {
      ...paramOptions.headers,
      ...contentTypeHeader,
    },
  };

  const adAuthenticatedFetch = createAdAuthenticatedFetch({
    homeAccountId: "",
    environment: "",
    tenantId: "",
    username: "",
  });
  return adAuthenticatedFetch(path, options).then((r) => r.json());
};

export const setCache = (path: string, data: any) => mutate(path, data, false);

export const globalTableOptions: Options<object> = {
  addRowPosition: "first",
};

export default function Tables() {
  return (
    <>
      <AssociatedTasksTable />
      <ProjectsTable />
      <TasksTable />
      <CustomerTable />
      <UserTable />
    </>
  );
}
