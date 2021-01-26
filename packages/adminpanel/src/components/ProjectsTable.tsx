import MaterialTable, { Column } from "material-table";
import React, { useContext } from "react";
import useSWR from "swr";
import { AlvtimeContext } from "../App";
import { norsk } from "./norsk";
import tableIcons from "./tableIcons";
import { globalTableOptions, setCache } from "./Tables";
import TasksTable from "./TasksTable";

export default function ProjectsTable(props: { customer: object }) {
  const customer: { id: number } = (props.customer as unknown) as {
    id: number;
  };
  const { alvtimeFetcher } = useContext(AlvtimeContext);
  const columns: Column<object>[] = [
    { title: "Navn", field: "name", editable: "always", type: "string" },
  ];

  const path = "/api/admin/Projects";

  const { data, error } = useSWR(path);

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, { ...newData, customer: { id: customer.id } }]);
    const addedData = await alvtimeFetcher(path, {
      method: "post",
      body: [
        {
          id: newData.id,
          name: newData.name,
          customer: customer.id, // The Id is set in the name field in the Autocomplete
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
      body: [
        { id: newData.id, name: newData.name, customer: newData.customer.id },
      ],
    });
    dataUpdate[index] = updatedData[0];
    setCache(path, [...dataUpdate]);
  };

  if (error) return <div>Error...</div>;
  const isLoading = !data;
  const filteredData = !data
    ? data
    : data.filter(
        (project: { customer: { id: number } }) =>
          project.customer.id === ((customer.id as unknown) as number)
      );

  return (
    <MaterialTable
      icons={tableIcons}
      title="Prosjekter"
      columns={columns}
      data={filteredData}
      isLoading={isLoading}
      options={{ ...globalTableOptions }}
      editable={{
        onRowAdd: handleRowAdd,
        onRowUpdate: handleRowUpdate,
      }}
      localization={norsk}
      detailPanel={(rowData) => {
        return (
          <div style={{ paddingLeft: "1rem" }}>
            <TasksTable project={rowData} />
          </div>
        );
      }}
    />
  );
}
