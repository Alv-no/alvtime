import MaterialTable, { Column } from "material-table";
import React from "react";
import useSWR from "swr";
import tableIcons from "./tableIcons";
import { fetcher, setCache, globalTableOptions } from "./Tables";
import { norsk } from "./norsk";

export default function HourRates() {
  const columns: Column<object>[] = [
    { title: "Aktivitet", field: "task.id", editable: "onAdd", type: "numeric" },
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
    { title: "Gjelder fra", field: "fromDate", editable: "onAdd", type: "date" },
  ];

  const path = "/api/admin/HourRates";

  const { data, error } = useSWR(path, fetcher);

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, newData]);
    const addedData = await fetcher(path, {
      method: "post",
      body: [
        {
          taskId: newData.task.id,
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
    const updatedData = await fetcher(path, {
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
