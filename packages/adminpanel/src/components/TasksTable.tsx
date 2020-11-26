import MaterialTable, { Column } from "material-table";
import React, { useContext, useState } from "react";
import useSWR from "swr";
import { AlvtimeContext } from "../App";
import { norsk } from "./norsk";
import tableIcons from "./tableIcons";
import { globalTableOptions, setCache } from "./Tables";
import { TextField } from "@material-ui/core";
import Autocomplete from "@material-ui/lab/Autocomplete";

export default function TasksTable() {
  const { alvtimeFetcher } = useContext(AlvtimeContext);

  const { data: projects, error: projectsLoadError } = useSWR(
    "/api/admin/Projects"
  );
  const path = "/api/admin/Tasks";
  const { data, error } = useSWR(path);

  const columns: Column<object>[] = [
    { title: "Navn", field: "name", editable: "always" },
    {
      title: "Prosjekt",
      field: "project.name",
      editable: "onAdd",
      type: "string",
      editComponent: (props: any) => {
        return (
          <Autocomplete
            options={projects}
            getOptionLabel={(option: { name: string }) => option.name}
            onChange={(
              _event: any,
              newValue: { name: string; id: number } | null
            ) => {
              props.onChange(newValue ? newValue.id : 0);
            }}
            renderInput={(params: any) => {
              return <TextField {...params} />;
            }}
          />
        );
      },
    },
    {
      title: "Kundenavn",
      field: "project.customer.name",
      editable: "never",
      type: "string",
    },
    {
      title: "Overtidsfaktor",
      field: "compensationRate",
      editable: "always",
      type: "numeric",
      initialEditValue: 1,
    },
    {
      title: "Aktiv",
      field: "isActive",
      editable: "always",
      type: "boolean",
      initialEditValue: true,
    },
  ];

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, newData]);
    const addedData = await alvtimeFetcher(path, {
      method: "post",
      body: [
        {
          ...newData,
          project: newData.project.name, // The Id is set in the name field in the Autocomplete
          description: "",
          locked: !newData.isActive,
        },
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
