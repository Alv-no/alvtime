import React from "react";
import { mutate } from "swr";
import { adAuthenticatedFetch } from "../services/azureAd";
import CustomerTable from "./CustomerTable";
import UserTable from "./UserTable";
import { Options } from "material-table";

export const fetcher = (url: string, options?: RequestInit) =>
  adAuthenticatedFetch(url, options).then((r) => r.json());

export const setCache = (path: string, data: any) => mutate(path, data, false);

export const globalTableOptions: Options<object> = {
  addRowPosition: "first",
};

export default function Tables() {
  return (
    <>
      <CustomerTable />
      <UserTable />
    </>
  );
}
