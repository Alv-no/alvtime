import MaterialTable, { Column } from "material-table";
import React, { useContext } from "react";
import useSWR from "swr";
import { AlvtimeContext } from "../App";
import { norsk } from "./norsk";
import tableIcons from "./tableIcons";
import { globalTableOptions, setCache } from "./Tables";

export default function UserTable() {
  const { alvtimeFetcher } = useContext(AlvtimeContext);
  const columns: Column<object>[] = [
    { title: "Navn", field: "name", editable: "always" },
    { title: "E-post", field: "email", editable: "always" },
    { title: "Start dato", field: "startDate", editable: "always" },
  ];

  const path = "/api/admin/Users";

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
      title="Ansatte"
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
