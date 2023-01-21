import MaterialTable, { Column } from "material-table";
import React, { useContext } from "react";
import useSWR from "swr";
import { AlvtimeContext } from "../App";
import { norsk } from "./norsk";
import tableIcons from "./tableIcons";
import { globalTableOptions, setCache } from "./Tables";
import { format } from "date-fns";

export default function HourRates(props: { task: object }) {
  const task = (props.task as unknown) as {
    id: number;
  };
  const { alvtimeFetcher } = useContext(AlvtimeContext);

  const columns: Column<object>[] = [
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
    setCache(path, [...data, { ...newData, task: { id: task.id } }]);
    const addedData = await alvtimeFetcher(path, {
      method: "post",
      body: [
        {
          taskId: task.id, // The Id is set in the name field in the Autocomplete
          fromDate: format(newData.fromDate, "yyyy-MM-dd"),
          rate: newData.rate,
        },
      ],
    });
    setCache(path, [...addedData, ...data]);
  };

  const handleRowUpdate = async (newData: any, oldData: any) => {
    const dataUpdate = [...data];
    const index = dataUpdate.findIndex((x) => x.id === oldData.id);
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
  const isLoading = !data;
  const filteredData = !data
    ? data
    : data
        .filter(
          (rate: { task: { id: number } }) =>
            rate.task.id === ((task.id as unknown) as number)
        )
        .sort((rateA: { fromDate: string }, rateB: { fromDate: string }) => {
          const dateA = new Date(rateA.fromDate);
          const dateB = new Date(rateB.fromDate);
          return dateB.getTime() - dateA.getTime();
        });

  return (
    <MaterialTable
      icons={tableIcons}
      title="Timerater"
      columns={columns}
      data={filteredData}
      isLoading={isLoading}
      options={{ ...globalTableOptions }}
      editable={{
        onRowAdd: handleRowAdd,
        onRowUpdate: handleRowUpdate,
      }}
      localization={norsk}
    />
  );
}
