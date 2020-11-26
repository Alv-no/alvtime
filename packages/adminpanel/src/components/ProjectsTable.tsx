import MaterialTable, { Column } from "material-table";
import React, { useContext } from "react";
import useSWR from "swr";
import { AlvtimeContext } from "../App";
import { norsk } from "./norsk";
import tableIcons from "./tableIcons";
import { globalTableOptions, setCache } from "./Tables";
import { TextField } from "@material-ui/core";
import Autocomplete from "@material-ui/lab/Autocomplete";

export default function ProjectsTable() {
  const { alvtimeFetcher } = useContext(AlvtimeContext);
  const { data: customers, error: customersLoadError } = useSWR(
    "/api/admin/Customers"
  );
  const columns: Column<object>[] = [
    { title: "Navn", field: "name", editable: "always", type: "string" },
    {
      title: "Kunde",
      field: "customer.name",
      editable: "onAdd",
      type: "string",
      editComponent: (props: any) => (
        <Autocomplete
          options={customers}
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
      ),
    },
  ];

  const path = "/api/admin/Projects";

  const { data, error } = useSWR(path);

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, newData]);
    const addedData = await alvtimeFetcher(path, {
      method: "post",
      body: [
        {
          id: newData.id,
          name: newData.name,
          customer: newData.customer.name, // The Id is set in the name field in the Autocomplete
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
      body: [
        { id: newData.id, name: newData.name, customer: newData.customer.id },
      ],
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
