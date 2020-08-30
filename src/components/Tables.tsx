import React from "react";
import { mutate } from "swr";
import { adAuthenticatedFetch } from "../services/azureAd";
import CustomerTable from "./CustomerTable";
import UserTable from "./UserTable";
import TasksTable from "./TasksTable";
import { Options } from "material-table";

export const fetcher = async (url: string, paramOptions: any) => {
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

  return adAuthenticatedFetch(url, options).then((r) => r.json());
};

export const setCache = (path: string, data: any) => mutate(path, data, false);

export const globalTableOptions: Options<object> = {
  addRowPosition: "first",
};

export default function Tables() {
  return (
    <>
      <TasksTable />
      <CustomerTable />
      <UserTable />
    </>
  );
}
