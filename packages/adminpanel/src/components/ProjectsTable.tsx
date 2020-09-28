import MaterialTable, { Column } from "material-table";
import React, { useContext } from "react";
import useSWR from "swr";
import { AlvtimeContext } from "../App";
import { norsk } from "./norsk";
import tableIcons from "./tableIcons";
import { globalTableOptions, setCache } from "./Tables";

export default function ProjectsTable() {
  const { alvtimeFetcher } = useContext(AlvtimeContext);
  const columns: Column<object>[] = [
    { title: "Navn", field: "name", editable: "always", type: "string" },
    {
      title: "Kunde Id",
      field: "customer.id",
      editable: "always",
      type: "numeric",
    },
    {
      title: "Kundenavn",
      field: "customer.name",
      editable: "never",
      type: "string",
    },
  ];

  const path = "/api/admin/Projects";

  const { data, error } = useSWR(path);

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, newData]);
    const addedData = await alvtimeFetcher(path, {
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
    const updatedData = await alvtimeFetcher(path, {
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
      title="Prosjekter"
      columns={columns}
      data={data}
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
