import MaterialTable, { Column } from "material-table";
import React, { useContext } from "react";
import useSWR from "swr";
import { AlvtimeContext } from "../App";
import { norsk } from "./norsk";
import tableIcons from "./tableIcons";
import { globalTableOptions, setCache } from "./Tables";

export default function TasksTable() {
  const { alvtimeFetcher } = useContext(AlvtimeContext);

  const columns: Column<object>[] = [
    { title: "Navn", field: "name", editable: "always" },
    {
      title: "Prosjekt Id",
      field: "project.id",
      editable: "onAdd",
      type: "numeric",
    },
    {
      title: "Prosjektnavn",
      field: "project.name",
      editable: "never",
      type: "string",
    },
    {
      title: "Kundenavn",
      field: "project.customer.name",
      editable: "never",
      type: "string",
    },
    {
      title: "Timerate",
      field: "compensationRate",
      editable: "always",
      type: "numeric",
    },
    { title: "Aktiv", field: "isActive", editable: "always", type: "boolean" },
  ];

  const path = "/api/admin/Tasks";

  const { data, error } = useSWR(path, alvtimeFetcher);

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, newData]);
    const addedData = await alvtimeFetcher(path, {
      method: "post",
      body: [
        { ...newData, project: newData.project.id, locked: !newData.isActive },
      ],
    });
    setCache(path, [...addedData, ...data]);
  };

  const handleRowUpdate = async (newData: any, oldData: any) => {
    const dataUpdate = [...data];
    const index = oldData.tableData.id;
    dataUpdate[index] = newData;
    setCache(path, [...dataUpdate]);
    const updatedData = await alvtimeFetcher(path, {
      method: "put",
      body: [{ ...newData, locked: !newData.isActive }],
    });
    dataUpdate[index] = updatedData[0];
    setCache(path, [...dataUpdate]);
  };

  const dataToShow = !data
    ? data
    : data.map((task: any) => ({
        ...task,
        isActive: !task.locked,
      }));

  if (error) return <div>Error...</div>;
  return (
    <MaterialTable
      icons={tableIcons}
      title="Aktiviteter"
      columns={columns}
      data={dataToShow}
      isLoading={!data}
      options={{ ...globalTableOptions }}
      editable={{
        onRowAdd: handleRowAdd,
        onRowUpdate: handleRowUpdate,
      }}
      localization={norsk}
    />
  );
}
