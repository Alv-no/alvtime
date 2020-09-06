import MaterialTable, { Column } from "material-table";
import React from "react";
import useSWR from "swr";
import tableIcons from "./tableIcons";
import { fetcher, setCache, globalTableOptions } from "./Tables";

export default function ProjectsTable() {
  const columns: Column<object>[] = [
    { title: "Navn", field: "name", editable: "always", type: "string" },
    {
      title: "Kunde",
      field: "customerName",
      editable: "always",
      type: "string",
    },
  ];

  const path = "/api/admin/Projects";

  const { data, error } = useSWR(path, fetcher);

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, newData]);
    const addedData = await fetcher("/api/admin/CreateProject", {
      method: "post",
      body: [newData],
    });
    setCache(path, [...addedData, ...data]);
  };

  const handleRowUpdate = async (newData: any, oldData: any) => {
    const dataUpdate = [...data];
    const index = oldData.tableData.id;
    dataUpdate[index] = newData;
    setCache(path, [...dataUpdate]);
    const updatedData = await fetcher(path, {
      method: "put",
      body: [newData],
    });
    dataUpdate[index] = updatedData[0];
    setCache(path, [...dataUpdate]);
  };

  const d = !data
    ? data
    : data.map((p: any) => ({ ...p, customerName: p.customer?.name }));

  if (error) return <div>Error...</div>;
  return (
    <MaterialTable
      icons={tableIcons}
      title="Projects"
      columns={columns}
      data={d}
      isLoading={!data}
      options={{ ...globalTableOptions }}
      editable={{
        onRowAdd: handleRowAdd,
        onRowUpdate: handleRowUpdate,
      }}
    />
  );
}
