import MaterialTable, { Column } from "material-table";
import React from "react";
import useSWR from "swr";
import tableIcons from "./tableIcons";
import { fetcher, setCache, globalTableOptions } from "./Tables";

export default function TasksTable() {
  const columns: Column<object>[] = [
    { title: "Navn", field: "name", editable: "always" },
    {
      title: "Prosjekt",
      field: "projectName",
      editable: "always",
      type: "numeric",
    },
    { title: "Customer", field: "customerName", editable: "never" },
    { title: "Beskrivelse", field: "description", editable: "onAdd" },
    {
      title: "Rate",
      field: "compensationRate",
      editable: "always",
      type: "numeric",
    },
    { title: "Låst", field: "locked", editable: "always", type: "boolean" },
  ];

  const path = "/api/user/Tasks";

  const { data, error } = useSWR(path, fetcher);

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, newData]);
    const addedData = await fetcher("/api/admin/TaskAdmin", {
      method: "post",
      body: [newData],
    });
    setCache(path, [...addedData, ...data]);
  };

  const d = !data
    ? data
    : data.map((t: any) => ({
        ...t,
        projectName: t.project?.name,
        customerName: t.project?.customer?.name,
      }));

  const handleRowUpdate = async (newData: any, oldData: any) => {
    const dataUpdate = [...data];
    const index = oldData.tableData.id;
    dataUpdate[index] = newData;
    setCache(path, [...dataUpdate]);
    const updatedData = await fetcher("/api/admin/TaskAdmin", {
      method: "put",
      body: [newData],
    });
    dataUpdate[index] = updatedData[0];
    setCache(path, [...dataUpdate]);
  };

  if (error) return <div>Error...</div>;
  return (
    <MaterialTable
      icons={tableIcons}
      title="Tasks"
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
