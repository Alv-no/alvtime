import MaterialTable from "material-table";
import React from "react";
import useSWR from "swr";
import tableIcons from "./tableIcons";
import { fetcher, setCache, globalTableOptions } from "./Tables";

export default function CustomerTable() {
  const columns = [
    { title: "Navn", field: "name" },
    { title: "e-post", field: "contactEmail" },
    { title: "kontakt", field: "contactPerson" },
    { title: "telefon", field: "contactPhone" },
    { title: "addresse", field: "invoiceAddress" },
  ];

  const path = "/api/admin/Customers";

  const { data, error } = useSWR(path, fetcher);

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, newData]);
    const addedData = await fetcher(path, {
      method: "post",
      body: JSON.stringify([newData]),
    });
    setCache(path, [...addedData, ...data]);
  };

  const handleRowUpdate = async (newData: any, oldData: any) => {
    const dataUpdate = [...data];
    const index = oldData.tableData.id;
    dataUpdate[index] = newData;
    setCache(path, [...dataUpdate]);
    const updatedData = await fetcher(path, {
      method: "put",
      body: JSON.stringify([newData]),
    });
    dataUpdate[index] = updatedData[0];
    setCache(path, [...dataUpdate]);
  };

  if (error) return <div>Error...</div>;
  return (
    <MaterialTable
      icons={tableIcons}
      title="Customers"
      columns={columns}
      data={data}
      isLoading={!data}
      options={{ ...globalTableOptions }}
      editable={{
        onRowAdd: handleRowAdd,
        onRowUpdate: handleRowUpdate,
      }}
    />
  );
}
