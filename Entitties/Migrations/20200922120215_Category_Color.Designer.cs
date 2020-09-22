﻿// <auto-generated />
using System;
using MahtaKala.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MahtaKala.Entities.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20200922120215_Category_Color")]
    partial class Category_Color
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("MahtaKala.Entities.Brand", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Name")
                        .HasColumnName("name")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.HasKey("Id")
                        .HasName("pk_brands");

                    b.ToTable("brands");
                });

            modelBuilder.Entity("MahtaKala.Entities.Category", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Color")
                        .HasColumnName("color")
                        .HasColumnType("character varying(30)")
                        .HasMaxLength(30);

                    b.Property<bool>("Disabled")
                        .HasColumnName("disabled")
                        .HasColumnType("boolean");

                    b.Property<string>("Image")
                        .HasColumnName("image")
                        .HasColumnType("text");

                    b.Property<int>("Order")
                        .HasColumnName("order")
                        .HasColumnType("integer");

                    b.Property<long?>("ParentId")
                        .HasColumnName("parent_id")
                        .HasColumnType("bigint");

                    b.Property<bool>("Published")
                        .HasColumnName("published")
                        .HasColumnType("boolean");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("title")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.HasKey("Id")
                        .HasName("pk_categories");

                    b.HasIndex("ParentId")
                        .HasName("ix_categories_parent_id");

                    b.ToTable("categories");
                });

            modelBuilder.Entity("MahtaKala.Entities.City", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("IsCenter")
                        .HasColumnName("is_center")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<long>("ProvinceId")
                        .HasColumnName("province_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id")
                        .HasName("pk_cities");

                    b.HasIndex("ProvinceId")
                        .HasName("ix_cities_province_id");

                    b.ToTable("cities");
                });

            modelBuilder.Entity("MahtaKala.Entities.Order", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime?>("ActualDeliveryDate")
                        .HasColumnName("actual_delivery_date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long?>("AddressId")
                        .HasColumnName("address_id")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("ApproximateDeliveryDate")
                        .HasColumnName("approximate_delivery_date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("CheckOutData")
                        .HasColumnName("check_out_data")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("DelivererNo")
                        .HasColumnName("deliverer_no")
                        .HasColumnType("text");

                    b.Property<DateTime?>("SendDate")
                        .HasColumnName("send_date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("State")
                        .HasColumnName("state")
                        .HasColumnType("integer");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnName("total_price")
                        .HasColumnType("numeric");

                    b.Property<string>("TrackNo")
                        .HasColumnName("track_no")
                        .HasColumnType("text");

                    b.Property<long>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id")
                        .HasName("pk_orders");

                    b.HasIndex("AddressId")
                        .HasName("ix_orders_address_id");

                    b.HasIndex("UserId")
                        .HasName("ix_orders_user_id");

                    b.ToTable("orders");
                });

            modelBuilder.Entity("MahtaKala.Entities.OrderItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("CharacteristicValues")
                        .HasColumnName("characteristic_values")
                        .HasColumnType("text");

                    b.Property<decimal>("FinalPrice")
                        .HasColumnName("final_price")
                        .HasColumnType("numeric");

                    b.Property<long>("OrderId")
                        .HasColumnName("order_id")
                        .HasColumnType("bigint");

                    b.Property<long>("ProductPriceId")
                        .HasColumnName("product_price_id")
                        .HasColumnType("bigint");

                    b.Property<int>("Quantity")
                        .HasColumnName("quantity")
                        .HasColumnType("integer");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnName("unit_price")
                        .HasColumnType("numeric");

                    b.HasKey("Id")
                        .HasName("pk_order_items");

                    b.HasIndex("OrderId")
                        .HasName("ix_order_items_order_id");

                    b.HasIndex("ProductPriceId")
                        .HasName("ix_order_items_product_price_id");

                    b.ToTable("order_items");
                });

            modelBuilder.Entity("MahtaKala.Entities.Payment", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("Amount")
                        .HasColumnName("amount")
                        .HasColumnType("numeric");

                    b.Property<long>("OrderId")
                        .HasColumnName("order_id")
                        .HasColumnType("bigint");

                    b.Property<string>("PayToken")
                        .HasColumnName("pay_token")
                        .HasColumnType("text");

                    b.Property<string>("ReferenceNumber")
                        .HasColumnName("reference_number")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<DateTime>("RegisterDate")
                        .HasColumnName("register_date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("State")
                        .HasColumnName("state")
                        .HasColumnType("integer");

                    b.Property<string>("TrackingNumber")
                        .HasColumnName("tracking_number")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<string>("UniqueId")
                        .HasColumnName("unique_id")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("pk_payments");

                    b.HasIndex("OrderId")
                        .HasName("ix_payments_order_id");

                    b.ToTable("payments");
                });

            modelBuilder.Entity("MahtaKala.Entities.Product", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("BrandId")
                        .HasColumnName("brand_id")
                        .HasColumnType("bigint");

                    b.Property<int?>("BuyQuotaDays")
                        .HasColumnName("buy_quota_days")
                        .HasColumnType("integer");

                    b.Property<string>("Characteristics")
                        .HasColumnName("characteristics")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnName("description")
                        .HasColumnType("text");

                    b.Property<string>("ImageList")
                        .HasColumnName("image_list")
                        .HasColumnType("text");

                    b.Property<int?>("MaxBuyQuota")
                        .HasColumnName("max_buy_quota")
                        .HasColumnType("integer");

                    b.Property<int?>("MinBuyQuota")
                        .HasColumnName("min_buy_quota")
                        .HasColumnType("integer");

                    b.Property<string>("Properties")
                        .HasColumnName("properties")
                        .HasColumnType("text");

                    b.Property<bool>("Published")
                        .HasColumnName("published")
                        .HasColumnType("boolean");

                    b.Property<long?>("SellerId")
                        .HasColumnName("seller_id")
                        .HasColumnType("bigint");

                    b.Property<int>("Status")
                        .HasColumnName("status")
                        .HasColumnType("integer");

                    b.Property<string>("Thubmnail")
                        .HasColumnName("thubmnail")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .HasColumnName("title")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.HasKey("Id")
                        .HasName("pk_products");

                    b.HasIndex("BrandId")
                        .HasName("ix_products_brand_id");

                    b.HasIndex("SellerId")
                        .HasName("ix_products_seller_id");

                    b.ToTable("products");
                });

            modelBuilder.Entity("MahtaKala.Entities.ProductCategory", b =>
                {
                    b.Property<long>("ProductId")
                        .HasColumnName("product_id")
                        .HasColumnType("bigint");

                    b.Property<long>("CategoryId")
                        .HasColumnName("category_id")
                        .HasColumnType("bigint");

                    b.HasKey("ProductId", "CategoryId")
                        .HasName("pk_product_category");

                    b.HasIndex("CategoryId")
                        .HasName("ix_product_category_category_id");

                    b.ToTable("product_category");
                });

            modelBuilder.Entity("MahtaKala.Entities.ProductPrice", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("CharacteristicValues")
                        .HasColumnName("characteristic_values")
                        .HasColumnType("text");

                    b.Property<decimal>("DiscountPrice")
                        .HasColumnName("discount_price")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Price")
                        .HasColumnName("price")
                        .HasColumnType("numeric");

                    b.Property<long>("ProductId")
                        .HasColumnName("product_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id")
                        .HasName("pk_product_prices");

                    b.HasIndex("ProductId")
                        .HasName("ix_product_prices_product_id");

                    b.ToTable("product_prices");
                });

            modelBuilder.Entity("MahtaKala.Entities.ProductQuantity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("CharacteristicValues")
                        .HasColumnName("characteristic_values")
                        .HasColumnType("text");

                    b.Property<long>("ProductId")
                        .HasColumnName("product_id")
                        .HasColumnType("bigint");

                    b.Property<int>("Quantity")
                        .HasColumnName("quantity")
                        .HasColumnType("integer");

                    b.HasKey("Id")
                        .HasName("pk_product_quantities");

                    b.HasIndex("ProductId")
                        .HasName("ix_product_quantities_product_id");

                    b.ToTable("product_quantities");
                });

            modelBuilder.Entity("MahtaKala.Entities.Province", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.HasKey("Id")
                        .HasName("pk_provinces");

                    b.ToTable("provinces");
                });

            modelBuilder.Entity("MahtaKala.Entities.RefreshToken", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("Created")
                        .HasColumnName("created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CreatedByIp")
                        .HasColumnName("created_by_ip")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<DateTime>("Expires")
                        .HasColumnName("expires")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("ReplacedByToken")
                        .HasColumnName("replaced_by_token")
                        .HasColumnType("text");

                    b.Property<DateTime?>("Revoked")
                        .HasColumnName("revoked")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("RevokedByIp")
                        .HasColumnName("revoked_by_ip")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<string>("Token")
                        .HasColumnName("token")
                        .HasColumnType("text");

                    b.Property<long>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id")
                        .HasName("pk_refresh_tokens");

                    b.HasIndex("UserId")
                        .HasName("ix_refresh_tokens_user_id");

                    b.ToTable("refresh_tokens");
                });

            modelBuilder.Entity("MahtaKala.Entities.Seller", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("AccountBankName")
                        .HasColumnName("account_bank_name")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<string>("AccountNumber")
                        .HasColumnName("account_number")
                        .HasColumnType("character varying(30)")
                        .HasMaxLength(30);

                    b.Property<string>("Name")
                        .HasColumnName("name")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<long?>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id")
                        .HasName("pk_sellers");

                    b.HasIndex("UserId")
                        .HasName("ix_sellers_user_id");

                    b.ToTable("sellers");
                });

            modelBuilder.Entity("MahtaKala.Entities.ShoppingCart", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Count")
                        .HasColumnName("count")
                        .HasColumnType("integer");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnName("date_created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long>("ProductPriceId")
                        .HasColumnName("product_price_id")
                        .HasColumnType("bigint");

                    b.Property<string>("SessionId")
                        .HasColumnName("session_id")
                        .HasColumnType("text");

                    b.Property<long?>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id")
                        .HasName("pk_shopping_carts");

                    b.HasIndex("ProductPriceId")
                        .HasName("ix_shopping_carts_product_price_id");

                    b.HasIndex("UserId")
                        .HasName("ix_shopping_carts_user_id");

                    b.ToTable("shopping_carts");
                });

            modelBuilder.Entity("MahtaKala.Entities.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("EmailAddress")
                        .HasColumnName("email_address")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<string>("FirstName")
                        .HasColumnName("first_name")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<string>("LastName")
                        .HasColumnName("last_name")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<string>("MobileNumber")
                        .HasColumnName("mobile_number")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<string>("NationalCode")
                        .HasColumnName("national_code")
                        .HasColumnType("character varying(10)")
                        .HasMaxLength(10);

                    b.Property<string>("Password")
                        .HasColumnName("password")
                        .HasColumnType("text");

                    b.Property<string>("SecurityStamp")
                        .HasColumnName("security_stamp")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnName("type")
                        .HasColumnType("integer");

                    b.Property<string>("Username")
                        .HasColumnName("username")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.ToTable("users");
                });

            modelBuilder.Entity("MahtaKala.Entities.UserActivationCode", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Code")
                        .HasColumnName("code")
                        .HasColumnType("integer");

                    b.Property<DateTime>("ExpireTime")
                        .HasColumnName("expire_time")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("IssueTime")
                        .HasColumnName("issue_time")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id")
                        .HasName("pk_user_activation_codes");

                    b.HasIndex("UserId")
                        .HasName("ix_user_activation_codes_user_id");

                    b.ToTable("user_activation_codes");
                });

            modelBuilder.Entity("MahtaKala.Entities.UserAddress", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("CityId")
                        .HasColumnName("city_id")
                        .HasColumnType("bigint");

                    b.Property<string>("Details")
                        .HasColumnName("details")
                        .HasColumnType("text");

                    b.Property<bool>("Disabled")
                        .HasColumnName("disabled")
                        .HasColumnType("boolean");

                    b.Property<double>("Lat")
                        .HasColumnName("lat")
                        .HasColumnType("double precision");

                    b.Property<double>("Lng")
                        .HasColumnName("lng")
                        .HasColumnType("double precision");

                    b.Property<string>("PostalCode")
                        .HasColumnName("postal_code")
                        .HasColumnType("character varying(10)")
                        .HasMaxLength(10);

                    b.Property<string>("Title")
                        .HasColumnName("title")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<long>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id")
                        .HasName("pk_addresses");

                    b.HasIndex("CityId")
                        .HasName("ix_addresses_city_id");

                    b.HasIndex("UserId")
                        .HasName("ix_addresses_user_id");

                    b.ToTable("addresses");
                });

            modelBuilder.Entity("MahtaKala.Entities.Wishlist", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("ProductId")
                        .HasColumnName("product_id")
                        .HasColumnType("bigint");

                    b.Property<long>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("bigint");

                    b.HasKey("Id")
                        .HasName("pk_wishlists");

                    b.HasIndex("ProductId")
                        .HasName("ix_wishlists_product_id");

                    b.HasIndex("UserId")
                        .HasName("ix_wishlists_user_id");

                    b.ToTable("wishlists");
                });

            modelBuilder.Entity("MahtaKala.Entities.Category", b =>
                {
                    b.HasOne("MahtaKala.Entities.Category", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId")
                        .HasConstraintName("fk_categories_categories_parent_id");
                });

            modelBuilder.Entity("MahtaKala.Entities.City", b =>
                {
                    b.HasOne("MahtaKala.Entities.Province", "Province")
                        .WithMany("Cities")
                        .HasForeignKey("ProvinceId")
                        .HasConstraintName("fk_cities_provinces_province_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MahtaKala.Entities.Order", b =>
                {
                    b.HasOne("MahtaKala.Entities.UserAddress", "Address")
                        .WithMany()
                        .HasForeignKey("AddressId")
                        .HasConstraintName("fk_orders_addresses_address_id");

                    b.HasOne("MahtaKala.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_orders_users_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MahtaKala.Entities.OrderItem", b =>
                {
                    b.HasOne("MahtaKala.Entities.Order", "Order")
                        .WithMany("Items")
                        .HasForeignKey("OrderId")
                        .HasConstraintName("fk_order_items_orders_order_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MahtaKala.Entities.ProductPrice", "ProductPrice")
                        .WithMany("OrderItems")
                        .HasForeignKey("ProductPriceId")
                        .HasConstraintName("fk_order_items_product_prices_product_price_id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("MahtaKala.Entities.Payment", b =>
                {
                    b.HasOne("MahtaKala.Entities.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId")
                        .HasConstraintName("fk_payments_orders_order_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MahtaKala.Entities.Product", b =>
                {
                    b.HasOne("MahtaKala.Entities.Brand", "Brand")
                        .WithMany()
                        .HasForeignKey("BrandId")
                        .HasConstraintName("fk_products_brands_brand_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MahtaKala.Entities.Seller", "Seller")
                        .WithMany()
                        .HasForeignKey("SellerId")
                        .HasConstraintName("fk_products_sellers_seller_id");
                });

            modelBuilder.Entity("MahtaKala.Entities.ProductCategory", b =>
                {
                    b.HasOne("MahtaKala.Entities.Category", "Category")
                        .WithMany("ProductCategories")
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("fk_product_category_categories_category_id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MahtaKala.Entities.Product", "Product")
                        .WithMany("ProductCategories")
                        .HasForeignKey("ProductId")
                        .HasConstraintName("fk_product_category_products_product_id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("MahtaKala.Entities.ProductPrice", b =>
                {
                    b.HasOne("MahtaKala.Entities.Product", "Product")
                        .WithMany("Prices")
                        .HasForeignKey("ProductId")
                        .HasConstraintName("fk_product_prices_products_product_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MahtaKala.Entities.ProductQuantity", b =>
                {
                    b.HasOne("MahtaKala.Entities.Product", "Product")
                        .WithMany("Quantities")
                        .HasForeignKey("ProductId")
                        .HasConstraintName("fk_product_quantities_products_product_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MahtaKala.Entities.RefreshToken", b =>
                {
                    b.HasOne("MahtaKala.Entities.User", "User")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_refresh_tokens_users_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MahtaKala.Entities.Seller", b =>
                {
                    b.HasOne("MahtaKala.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_sellers_users_user_id");
                });

            modelBuilder.Entity("MahtaKala.Entities.ShoppingCart", b =>
                {
                    b.HasOne("MahtaKala.Entities.ProductPrice", "ProductPrice")
                        .WithMany()
                        .HasForeignKey("ProductPriceId")
                        .HasConstraintName("fk_shopping_carts_product_prices_product_price_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MahtaKala.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_shopping_carts_users_user_id");
                });

            modelBuilder.Entity("MahtaKala.Entities.UserActivationCode", b =>
                {
                    b.HasOne("MahtaKala.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_user_activation_codes_users_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MahtaKala.Entities.UserAddress", b =>
                {
                    b.HasOne("MahtaKala.Entities.City", "City")
                        .WithMany()
                        .HasForeignKey("CityId")
                        .HasConstraintName("fk_addresses_cities_city_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MahtaKala.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_addresses_users_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MahtaKala.Entities.Wishlist", b =>
                {
                    b.HasOne("MahtaKala.Entities.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .HasConstraintName("fk_wishlists_products_product_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MahtaKala.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_wishlists_users_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
