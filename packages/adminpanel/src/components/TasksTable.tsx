import MaterialTable, { Column } from "material-table";
import React, { useContext } from "react";
import useSWR from "swr";
import { AlvtimeContext } from "../App";
import { norsk } from "./norsk";
import tableIcons from "./tableIcons";
import { globalTableOptions, setCache } from "./Tables";
import HourRates from "./HourRatesTable";

export default function TasksTable(props: { project: object }) {
  const project = (props.project as unknown) as {
    id: number;
  };
  const { alvtimeFetcher } = useContext(AlvtimeContext);

  const path = "/api/admin/Tasks";
  const { data, error } = useSWR(path);
  console.log(data);

  const columns: Column<object>[] = [
    { title: "Navn", field: "name", editable: "always" },
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
    setCache(path, [...data, { ...newData, project: { id: project.id } }]);
    const addedData = await alvtimeFetcher(path, {
      method: "post",
      body: [
        {
          ...newData,
          project: project.id, // The Id is set in the name field in the Autocomplete
          description: "",
          locked: !newData.isActive,
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
      method: "put",
      body: [{ ...newData, locked: !newData.isActive }],
    });
    dataUpdate[index] = updatedData[0];
    setCache(path, [...dataUpdate]);
  };

  const dataToShow = !data
    ? data
    : data
        .map((task: any) => ({
          ...task,
          isActive: !task.locked,
        }))
        .filter(
          (task: { project: { id: number } }) =>
            task.project.id === ((project.id as unknown) as number)
        );

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
      detailPanel={(rowData) => {
        return (
          <div style={{ paddingLeft: "1rem" }}>
            <HourRates task={rowData} />
          </div>
        );
      }}
    />
  );
}
