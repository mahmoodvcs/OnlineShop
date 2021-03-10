#!/bin/bash

echo "Reading data from Eskad DB ..."
#/srv/eskad-extractor/ExportSql2Pg
/srv/eskad-data-bringer/mahta-b-c-shop-backend/ExportSql2Pg-DotNetCore/bin/Debug/netcoreapp3.1/ExportSql2Pg-DotNetCore

echo "Updating Eskad products ..."
product_quantities_table="product_quantities"
product_prices_table="product_prices"
product_tags_table="product_tags"
product_payment_parties_table="product_payment_parties"
products_table="products"
connectionString="mahta:sry2k3344@172.16.7.74/mahta"

# Disabling old products;
#disable_old_products="update $products_table set status = 2 where seller_id = 54 and code not in (select substring(code from 2 for 6) from merchandise);"
#update_disabled_products_qunatity="update $product_quantities_table set quantity = 0 where product_id in (select id from $products_table where seller_id = 54 and code not in (select substring(code from 2 for 6) from merchandise));"
#handle_removed_products="$disable_old_products$update_disabled_products_qunatity"
handle_removed_products=""

#Updating product quantity
#enable_old_products="update $products_table set status = 0 where seller_id = 54 and code in (select substring(code from 2 for 6) from merchandise);"
#set_old_item_quantities="update $product_quantities_table set quantity = s.count from (select $products_table.id, count from merchandise join $products_table on $products_table.code = substring(merchandise.code from 2 for 6) where seller_id = 54) as s where s.id = product_id;"
set_old_item_prices="update $product_prices_table set price = s.price, discount_price = s.price from (select $products_table.id, price from merchandise join $products_table on $products_table.code = substring(merchandise.code from 2 for 6) where seller_id = 54) as s where s.id = product_id;"
handle_old_products="$enable_old_products$set_old_item_quantities$set_old_item_prices"

# Handling new products
insert_new_items="insert into $products_table (title, brand_id, seller_id, status, published, code) select name, 68, 54, 1000, 'f', substring(code from 2 for 6) from merchandise where substring(code from 2 for 6) not in (select code from $products_table where seller_id = 54);"
set_new_item_tags="insert into $product_tags_table select id, 2140 from $products_table where status = 1000;"
set_new_item_quantities=""
#insert into $product_quantities_table (product_id, quantity) select $products_table.id, count from merchandise join $products_table on $products_table.code = substring(merchandise.code from 2 for 6) where seller_id = 54 and status = 1000;"
set_new_item_prices="insert into $product_prices_table (product_id, price, discount_price) select $products_table.id, price, price from merchandise join $products_table on $products_table.code = substring(merchandise.code from 2 for 6) where seller_id = 54 and status = 1000;"
set_new_item_payment_share="insert into $product_payment_parties_table select id, 8, 100 from $products_table where status = 1000;"
update_new_item_status="update $products_table set status = 0 where status = 1000;"
handle_new_products="$insert_new_items$set_new_item_tags$set_new_item_quantities$set_new_item_prices$set_new_item_payment_share$update_new_item_status"

# Running the query
psql "postgresql://$connectionString" -c "begin; $handle_removed_products $handle_old_products $handle_new_products commit;"
