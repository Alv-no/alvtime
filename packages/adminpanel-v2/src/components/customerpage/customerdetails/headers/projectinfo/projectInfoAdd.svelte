<script lang="ts">
	import type { TCustomer, TProject } from "$lib/types";
    import { customers } from "../../../../../stores/CustomerStore";
    
    
    let activeCustomer: TCustomer | undefined = customers.getActiveCustomer();
    if (activeCustomer === undefined) {
        throw new Error("Not in valid state of program, can't pick project without active customer");
    }

    const project: TProject = {
        Id: Date.now(),
        Name: "",
        ProjectNumber: 1,
        Customer: activeCustomer.Id,
        Task: []
    };
    
    
    customers.addNewProject(project);
    customers.setProject(project.Id);
    $: reactiveProject = project;
    
    const addStyling: string ="rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent"

</script>

<p>
    Prosjekt:
    <input type="text" bind:value={reactiveProject.Name} class={addStyling}/>
</p>
<p>
    Prosjektnr:
    <input type="text" bind:value={reactiveProject.ProjectNumber} class={addStyling}/>
</p>
