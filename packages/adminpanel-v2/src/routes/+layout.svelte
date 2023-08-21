<script>
	import '../app.css';
    import 'iconify-icon'
    import SidebarEntry from "../components/sidebar/Sidebar.svelte"

    let hover = false
</script>

<div class="wrapper">
    <nav on:mouseenter={() => {
        hover = true;
    }}
    on:mouseleave={() => {
        hover = false;
    }}>
        <div class="container">
            {#if hover}
                <SidebarEntry page_name={"Home"} page_route={"/"} icon={"majesticons:home"}/>
                <SidebarEntry page_name={"Kunder"} page_route={"/customers"} icon={"ic:round-corporate-fare"}/>
                <SidebarEntry page_name={"Ansatte"} page_route={"/employees"} icon={"fluent:person-20-filled"}/>
            {:else}
                <SidebarEntry page_name={""} page_route={"/"} icon={"ph:plus-fill"}/>
            {/if}
            </div>
    </nav>

    <slot />
</div>

<style>
    .container {
        display: flex;
        flex-direction: column;
    }

    .wrapper {
        display: flex;
    }
</style>
