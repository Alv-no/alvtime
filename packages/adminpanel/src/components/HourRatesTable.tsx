import MaterialTable, { Column } from "material-table";
import React, { useContext } from "react";
import useSWR from "swr";
import { AlvtimeContext } from "../App";
import { norsk } from "./norsk";
import tableIcons from "./tableIcons";
import { globalTableOptions, setCache } from "./Tables";

export default function HourRates() {
  const { alvtimeFetcher } = useContext(AlvtimeContext);
  const columns: Column<object>[] = [
    {
      title: "Aktivitet",
      field: "task.id",
      editable: "onAdd",
      type: "numeric",
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
