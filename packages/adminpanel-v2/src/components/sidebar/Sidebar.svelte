<script lang="ts">
    import 'iconify-icon'

    export let page_name : string 
    export let page_route : string
    export let icon : string
</script>

<a href={page_route} class="Sidebar-entry no-underline hover:underline">
    <iconify-icon icon={icon} class="text-alv-dark-blue" width="30px"></iconify-icon>
    <div class="text-alv-dark-blue">
        {page_name}
    </div>
    
</a>

<style>
    .Sidebar-entry {
        display: flex;
        vertical-align: center;
    }
</style>