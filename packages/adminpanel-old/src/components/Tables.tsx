import { Options } from "material-table";
import React from "react";
import { mutate } from "swr";
import CustomerTable from "./CustomerTable";
import UserTable from "./UserTable";

export const setCache = (path: string, data: any) => mutate(path, data, false);

export const globalTableOptions: Options<object> = {
  addRowPosition: "first",
  paging:true,
  pageSize:20,       // make initial page size
  emptyRowsWhenPaging: false,   // To avoid of having empty rows
  pageSizeOptions:[5,10,20,50]
};

export default function Tables() {
  return (
    <>
      <UserTable />
      <CustomerTable />
    </>
  );
}
