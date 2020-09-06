import MaterialTable, { Column } from "material-table";
import React from "react";
import useSWR from "swr";
import tableIcons from "./tableIcons";
import { fetcher, setCache, globalTableOptions } from "./Tables";

export default function UserTable() {
  const columns: Column<object>[] = [
    { title: "Navn", field: "name", editable: "always" },
    { title: "E-post", field: "email", editable: "always" },
    { title: "Start dato", field: "startDate", editable: "always" },
  ];

  const path = "/api/admin/Users";

  const { data, error } = useSWR(path, fetcher);

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, newData]);
    const addedData = await fetcher("/api/admin/CreateUser", {
      method: "post",
      body: [newData],
    });
    setCache(path, [...addedData, ...data]);
  };

  if (error) return <div>Error...</div>;
  return (
    <MaterialTable
      icons={tableIcons}
      title="Users"
      columns={columns}
      data={data}
      isLoading={!data}
      options={{ ...globalTableOptions }}
      editable={{
        onRowAdd: handleRowAdd,
      }}
    />
  );
}
