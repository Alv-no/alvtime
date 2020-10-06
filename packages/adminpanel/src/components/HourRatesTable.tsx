import MaterialTable, { Column } from "material-table";
import React, { useContext } from "react";
import useSWR from "swr";
import { AlvtimeContext } from "../App";
import { norsk } from "./norsk";
import tableIcons from "./tableIcons";
import { globalTableOptions, setCache } from "./Tables";
import Autocomplete from "@material-ui/lab/Autocomplete";
import { TextField } from "@material-ui/core";

export default function HourRates() {
  const { alvtimeFetcher } = useContext(AlvtimeContext);
  const { data: tasks, error: tasksLoadError } = useSWR("/api/admin/Tasks");

  const columns: Column<object>[] = [
    {
      title: "Aktivitet",
      field: "task.name",
      editable: "onAdd",
      type: "string",
      editComponent: (props: any) => {
        return (
          <Autocomplete
            options={tasks}
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
      title: "Prosjektnavn",
      field: "task.project.name",
      editable: "never",
      type: "string",
    },
    {
      title: "Kundenavn",
      field: "task.project.customer.name",
      editable: "never",
      type: "string",
    },
    { title: "Timerate", field: "rate", editable: "always", type: "numeric" },
    {
      title: "Gjelder fra",
      field: "fromDate",
      editable: "onAdd",
      type: "date",
    },
  ];

  const path = "/api/admin/HourRates";

  const { data, error } = useSWR(path);

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, newData]);
    const addedData = await alvtimeFetcher(path, {
      method: "post",
      body: [
        {
          taskId: newData.task.name, // The Id is set in the name field in the Autocomplete
          fromDate: newData.fromDate,
          rate: newData.rate,
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
      method: "post",
      body: [
        {
          taskId: newData.task.id,
          fromDate: newData.fromDate,
          rate: newData.rate,
        },
      ],
    });
    dataUpdate[index] = updatedData[0];
    setCache(path, [...dataUpdate]);
  };

  if (error) return <div>Error...</div>;
  return (
    <MaterialTable
      icons={tableIcons}
      title="Timerater"
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
