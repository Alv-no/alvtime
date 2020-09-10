import MaterialTable, { Column } from "material-table";
import React, { useState } from "react";
import useSWR from "swr";
import tableIcons from "./tableIcons";
import TextField from "@material-ui/core/TextField";
import Autocomplete from "@material-ui/lab/Autocomplete";
import { fetcher, setCache, globalTableOptions } from "./Tables";

export default function TasksTable() {
  const [selectedCustomer, setSelectedCustomer] = useState("");

  const { data: projects, error: projError } = useSWR(
    "/api/admin/Projects",
    fetcher
  );
  const { data: customers, error: custError } = useSWR(
    "/api/admin/Customers",
    fetcher
  );

  const [value, setValue] = React.useState<any>(null);
  const [inputValue, setInputValue] = React.useState("");

  const columns: Column<object>[] = [
    { title: "Navn", field: "name", editable: "always" },
    {
      title: "Prosjekt",
      field: "projectName",
      editable: "onAdd",
      editComponent: (props) => (
        <Autocomplete
          options={projects.filter((project: any) =>
            value ? project.customer.id === value.id : true
          )}
          getOptionLabel={(option: any) => option.name}
          renderInput={(params: any) => <TextField {...params} />}
        />
      ),
    },
    {
      title: "Customer",
      field: "customerName",
      editable: "onAdd",
      editComponent: (props) => (
        <Autocomplete
          options={customers}
          getOptionLabel={(option: any) => option.name}
          value={value}
          onChange={(event: any, newValue: any) => {
            console.log("newValue: ", newValue);
            setValue(newValue);
          }}
          inputValue={inputValue}
          onInputChange={(event: any, newInputValue: any) => {
            console.log("newInputValue: ", newInputValue);
            setInputValue(newInputValue);
          }}
          renderInput={(params: any) => <TextField {...params} />}
        />
      ),
    },
    { title: "Beskrivelse", field: "description", editable: "onAdd" },
    {
      title: "Rate",
      field: "compensationRate",
      editable: "always",
      type: "numeric",
    },
    { title: "Låst", field: "locked", editable: "always", type: "boolean" },
  ];

  const path = "/api/admin/Tasks";

  const { data, error } = useSWR(path, fetcher);

  const handleRowAdd = async (newData: any) => {
    setCache(path, [...data, newData]);
    const addedData = await fetcher(path, {
      method: "post",
      body: [newData],
    });
    setCache(path, [...addedData, ...data]);
  };

  const d = !data
    ? data
    : data.map((t: any) => ({
        ...t,
        projectName: t.project?.name,
        customerName: t.project?.customer?.name,
      }));

  const handleRowUpdate = async (newData: any, oldData: any) => {
    const dataUpdate = [...data];
    const index = oldData.tableData.id;
    dataUpdate[index] = newData;
    setCache(path, [...dataUpdate]);
    const updatedData = await fetcher(path, {
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
      title="Tasks"
      columns={columns}
      data={d}
      isLoading={!data}
      options={{ ...globalTableOptions }}
      editable={{
        onRowAdd: handleRowAdd,
        onRowUpdate: handleRowUpdate,
      }}
    />
  );
}
