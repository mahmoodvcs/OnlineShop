-- After connecting to the database using psql, use the \copy command, like so:
-- \copy (
select pr.title as product, pr.id as product_id, pr.code as product_code, oi.quantity as order_quantity, o.check_out_date as order_date, case o.state when 2 then 'PAID' when 3 then 'SENT' when 4 then 'DELIVERED' end as order_state, pp.price from orders o inner join order_items oi on o.id = oi.order_id inner join product_prices pp on oi.product_price_id = pp.id inner join products pr on pp.product_id = pr.id where o.state in(2, 3, 4) order by o.check_out_date desc 
-- ) TO 'file.csv' CSV HEADER;